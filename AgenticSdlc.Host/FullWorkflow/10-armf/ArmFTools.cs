using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.ArmF;

/// <summary>
/// Der Werkzeugkasten des freien Ziel-Agenten (W2 Arm F) — Submit-Once mit harter deterministischer
/// Validierung (das bewiesene check-vor-save-Muster der Analyst-Linsen): Aussagen ohne existierende
/// AU-Referenz, mit leerem Statement oder ungültigem Typ kommen als Fehlerliste zurück, der Agent
/// iteriert selbst. Die Tools zählen zugleich das Ebene-C-Prozessprofil (§14.5) — rein beobachtbar:
/// Quell-Nachzugriffe, Validierungsaufrufe, Einreichungen, Selbstrevisionen (Draft-Hash-Wechsel).
/// Budget: nach <c>toolBudget</c> Aufrufen liefern Lese-/Prüf-Tools BUDGET_EXHAUSTED —
/// submit_result bleibt IMMER erlaubt (ein Budget darf die Abgabe nicht verhindern).
/// </summary>
public sealed class ArmFTools(IReadOnlyList<AtomicUnit> units, int toolBudget)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;
    private static readonly IReadOnlySet<string> Typen =
        new HashSet<string>(StringComparer.Ordinal) { "requirement", "architecture", "decision", "open_question", "risk" };
    private static readonly IReadOnlySet<string> Derivationen =
        new HashSet<string>(StringComparer.Ordinal) { "explicit", "derived" };
    private static readonly IReadOnlySet<string> Unsicherheiten =
        new HashSet<string>(StringComparer.Ordinal) { "none", "uncertain" };

    private static readonly IReadOnlySet<string> Dispositionen =
        new HashSet<string>(StringComparer.Ordinal) { "used", "non_relevant", "unresolved" };

    private readonly IReadOnlySet<string> _unitIds = units.Select(u => u.Id).ToHashSet(StringComparer.Ordinal);
    private string? _letzterDraftHash;

    public IReadOnlyList<ArmFStatement>? Submitted { get; private set; }
    public IReadOnlyList<ArmFBilanzEintrag>? SubmittedBilanz { get; private set; }
    public int ToolCalls { get; private set; }
    public int SourceReads { get; private set; }
    public int ValidationCalls { get; private set; }
    public int Submissions { get; private set; }
    public int SelfRevisions { get; private set; }
    public bool BudgetExhausted { get; private set; }

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ReadTranscript, "read_transcript",
            "Liest das kanonisch nummerierte Transkript erneut (optional nur den Bereich fromUnit..toUnit, "
            + "1-basierte Unit-Nummern). Nutze es, wann immer du etwas nachpruefen willst."),
        AIFunctionFactory.Create(ValidateDraft, "validate_draft",
            "Prueft Entwurf + Quellenbilanz deterministisch (Schema, Typen, existierende AU-Referenzen, "
            + "Duplikate, Bilanz-Vollstaendigkeit) und gibt eine Fehlerliste zurueck — beliebig oft nutzbar."),
        AIFunctionFactory.Create(SubmitResult, "submit_result",
            "GENAU EINMAL am Ende: reicht Endergebnis + vollstaendige Quellenbilanz ein. Validierung ist "
            + "HART (Fehlerliste zurueck = nachbessern und erneut einreichen). Erst eine GUELTIGE "
            + "Einreichung beendet die Aufgabe."),
    ];

    /// <summary>Deterministische Darstellung der kanonischen Quelle — identisch für Agent-Prompt und Re-Read.</summary>
    public static string Render(IEnumerable<AtomicUnit> auswahl)
        => string.Join("\n", auswahl.Select(u => $"[{u.Id}] {u.Speaker}: {u.Text}"));

    private string ReadTranscript(int? fromUnit = null, int? toUnit = null)
    {
        if (Verbraucht("read_transcript") is { } halt) return halt;
        SourceReads++;
        var von = Math.Max(1, fromUnit ?? 1);
        var bis = Math.Min(units.Count, toUnit ?? units.Count);
        if (von > bis) return JsonSerializer.Serialize(new { error = "RANGE_INVALID", units = units.Count }, Json);
        return Render(units.Skip(von - 1).Take(bis - von + 1));
    }

    private string ValidateDraft(IReadOnlyList<ArmFStatement> statements, IReadOnlyList<ArmFBilanzEintrag>? quellenbilanz = null)
    {
        if (Verbraucht("validate_draft") is { } halt) return halt;
        ValidationCalls++;
        ZaehleRevision(statements);
        var errors = Validate(statements);
        if (quellenbilanz is not null) errors.AddRange(ValidateBilanz(statements, quellenbilanz));
        return errors.Count > 0
            ? JsonSerializer.Serialize(new { valid = false, details = errors }, Json)
            : JsonSerializer.Serialize(new { valid = true, count = statements.Count }, Json);
    }

    private string SubmitResult(IReadOnlyList<ArmFStatement> statements, IReadOnlyList<ArmFBilanzEintrag> quellenbilanz)
    {
        ToolCalls++;   // submit zählt mit, wird aber nie vom Budget geblockt.
        Submissions++;
        ZaehleRevision(statements);
        var errors = Validate(statements);
        errors.AddRange(ValidateBilanz(statements, quellenbilanz));
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "RESULT_INVALID", details = errors }, Json);
        if (Submitted is not null) return JsonSerializer.Serialize(new { error = "ALREADY_SUBMITTED", hint = "submit_result ist GENAU EINMAL erlaubt." }, Json);

        Submitted = statements;
        SubmittedBilanz = quellenbilanz;
        return JsonSerializer.Serialize(new { submitted = true, count = statements.Count, bilanz = quellenbilanz.Count }, Json);
    }

    /// <summary>F-v2-Vertrag (Konzept §11): jede AU genau EINMAL disponiert; used→existierende
    /// Claim-IDs; non_relevant/unresolved OHNE Claim-IDs. Deterministisch, keine Semantik-Prüfung.</summary>
    private List<string> ValidateBilanz(IReadOnlyList<ArmFStatement> statements, IReadOnlyList<ArmFBilanzEintrag> bilanz)
    {
        var errors = new List<string>();
        var claimIds = statements.Select(s => s.Id).Where(i => !string.IsNullOrWhiteSpace(i)).ToHashSet(StringComparer.Ordinal);
        var gesehen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var b in bilanz)
        {
            if (!_unitIds.Contains(b.AuId)) { errors.Add($"Bilanz: unbekannte AU '{b.AuId}'."); continue; }
            if (!gesehen.Add(b.AuId)) errors.Add($"Bilanz: AU '{b.AuId}' mehrfach disponiert.");
            if (!Dispositionen.Contains(b.Disposition))
                errors.Add($"Bilanz {b.AuId}: disposition '{b.Disposition}' (erlaubt: used|non_relevant|unresolved).");
            if (b.Disposition == "used")
            {
                if (b.ClaimIds is not { Count: > 0 }) errors.Add($"Bilanz {b.AuId}: 'used' verlangt mind. 1 claimId.");
                else foreach (var c in b.ClaimIds.Where(c => !claimIds.Contains(c)))
                    errors.Add($"Bilanz {b.AuId}: claimId '{c}' existiert nicht im eingereichten Bestand.");
            }
            else if (b.ClaimIds is { Count: > 0 })
                errors.Add($"Bilanz {b.AuId}: '{b.Disposition}' darf keine claimIds tragen (widerspruechliche Disposition).");
        }
        foreach (var fehlt in _unitIds.Where(u => !gesehen.Contains(u)).OrderBy(u => u, StringComparer.Ordinal).Take(8))
            errors.Add($"Bilanz: AU '{fehlt}' fehlt — JEDE AU braucht genau eine Disposition.");
        var fehlend = _unitIds.Count(u => !gesehen.Contains(u));
        if (fehlend > 8) errors.Add($"Bilanz: … insgesamt {fehlend} AUs ohne Disposition.");
        return errors;
    }

    private List<string> Validate(IReadOnlyList<ArmFStatement> statements)
    {
        var errors = new List<string>();
        var gesehen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < statements.Count; i++)
        {
            var s = statements[i];
            if (string.IsNullOrWhiteSpace(s.Id)) errors.Add($"#{i + 1}: id PFLICHT (eindeutige Claim-ID für die Quellenbilanz).");
            else if (!ids.Add(s.Id)) errors.Add($"#{i + 1}: id '{s.Id}' doppelt vergeben.");
            if (string.IsNullOrWhiteSpace(s.Statement)) errors.Add($"#{i + 1}: statement leer.");
            else if (!gesehen.Add(s.Statement.Trim())) errors.Add($"#{i + 1}: exaktes Duplikat von statement '{Kurz(s.Statement)}'.");
            if (!Typen.Contains(s.Type)) errors.Add($"#{i + 1}: type '{s.Type}' (erlaubt: {string.Join("|", Typen)}).");
            if (!Derivationen.Contains(s.Derivation)) errors.Add($"#{i + 1}: derivation '{s.Derivation}' (erlaubt: explicit|derived).");
            if (!Unsicherheiten.Contains(s.Uncertainty)) errors.Add($"#{i + 1}: uncertainty '{s.Uncertainty}' (erlaubt: none|uncertain).");
            if (s.SourceUnitIds is not { Count: > 0 })
                errors.Add($"#{i + 1}: EVIDENZ-PFLICHT — mind. 1 sourceUnitId ([AU-....] aus dem Transkript).");
            else
            {
                var unbekannt = s.SourceUnitIds.Where(id => !_unitIds.Contains(id)).ToList();
                if (unbekannt.Count > 0) errors.Add($"#{i + 1}: sourceUnitIds unbekannt: {string.Join(", ", unbekannt)}.");
            }
        }
        return errors;
    }

    /// <summary>§14.5-Messregel Selbstrevision (rein beobachtbar): ein neuer Draft mit VERÄNDERTEM Inhalt
    /// gegenüber dem vorherigen Draft zählt als eine Revision — egal ob via validate_draft oder submit_result.</summary>
    private void ZaehleRevision(IReadOnlyList<ArmFStatement> statements)
    {
        var hash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(statements, Json))));
        if (_letzterDraftHash is not null && !string.Equals(_letzterDraftHash, hash, StringComparison.Ordinal))
            SelfRevisions++;
        _letzterDraftHash = hash;
    }

    private string? Verbraucht(string tool)
    {
        ToolCalls++;
        if (ToolCalls <= toolBudget) return null;
        BudgetExhausted = true;
        return JsonSerializer.Serialize(new
        { error = "BUDGET_EXHAUSTED", hint = $"Werkzeug-Budget ({toolBudget}) erschoepft — reiche dein Ergebnis JETZT mit submit_result ein.", tool }, Json);
    }

    private static string Kurz(string s) => s.Length <= 60 ? s : s[..60] + "…";
}

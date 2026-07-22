using System.Text.RegularExpressions;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.FullWorkflow.Ledger;

namespace AgenticSdlc.Host.FullWorkflow.MakerChecker;

/// <summary>
/// MC0 — der deterministische Contract-Checker. Reine C#-Logik, KEIN LLM, KEIN Workflow:
/// <c>(requirements.md mit [ids], consumable.json) -&gt; ContractCheckReport</c>.
/// Portiert die <c>eval-mini-ab.py</c>-Regeln (K1/K2/K4 = C1/C2/C5) und ergänzt C4 (Disposition)
/// sowie C3 als nicht-blockierende Warning-Heuristik. Das Ergebnis ist das Review-Zertifikat
/// (Contract.md §2) und zugleich das Messinstrument, das entscheidet, ob ein Repair-Loop nötig ist.
/// </summary>
public static class ContractChecker
{
    // Markdown-Listenpunkt = eine (fachliche) Anforderungszeile.
    private static readonly Regex ReqLineRx = new(@"^\s*[-*]\s+", RegexOptions.Compiled);
    // [id] bzw. [id1, id2]; reine Zahlen ([1]) sind KEINE Claim-IDs (Fußnoten/Aufzählung).
    private static readonly Regex IdRx = new(@"\[([a-zA-Z0-9_\-, ]+)\]", RegexOptions.Compiled);

    // C3-Heuristik: harte Verstärkungssprache vs. weiche Absicherung (grob, bewusst konservativ).
    private static readonly Regex HardRx = new(
        @"\b(muss|müssen|verpflichtend|zwingend|zwingend erforderlich|ist erforderlich|entschieden|festgelegt|im MVP|MVP-)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SoftRx = new(
        @"\b(soll|sollte|sollen|gewünscht|wunsch|optional|kann|könnte|offen|unklar|geplant|später|ggf|eventuell|möglicherweise|zu klären|prüfen)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Weiche Facetten: hier darf das Artefakt nicht "muss/entschieden/MVP" behaupten.
    private static readonly HashSet<string> SoftStatus = new(StringComparer.OrdinalIgnoreCase) { "open", "uncertain" };
    private static readonly HashSet<string> SoftModality = new(StringComparer.OrdinalIgnoreCase)
        { "desired", "optional", "must_note", "must_clarify", "must_consider" };
    private static readonly HashSet<string> SoftTime = new(StringComparer.OrdinalIgnoreCase)
        { "later_possible", "mvp_or_later_unclear" };

    /// <param name="iteration">1-basiert; im Loop später hochgezählt. In MC0 (kein Loop) = 1.</param>
    /// <param name="maxIterations">Loop-Obergrenze; steuert nur die MaxIterationsReached-Entscheidung.</param>
    /// <param name="artifactDisposition">Ledger-Disposition-Key des Zielartefakts (requirements|risks|architecture|open-questions).</param>
    public static ContractCheckReport Check(string markdown, ConsumableLedger ledger,
        int iteration = 1, int maxIterations = 3, string artifactDisposition = "requirements")
    {
        var claims = new Dictionary<string, SemanticLedgerEntry>(StringComparer.Ordinal);
        foreach (var c in ledger.Claims)
            claims[c.Id] = c; // "last wins" — consumable ist dedupliziert, defensiv trotzdem robust.

        var lines = ParseRequirementLines(markdown);
        var violations = new List<ContractViolation>();
        var citedIds = new HashSet<string>(StringComparer.Ordinal);

        var linesWithCitation = 0;
        var unknownIdCount = 0;
        var wrongDispCount = 0;
        var facetCandidates = 0;

        foreach (var (lineNo, text, ids) in lines)
        {
            if (ids.Count == 0)
            {
                // C1 — fachliche Zeile ohne Quelle. Nicht sicher auto-reparierbar -> HumanReview.
                violations.Add(new ContractViolation(
                    ContractCodes.MissingCitation, ContractSeverity.Error, Repairable: false,
                    Message: "Anforderungszeile ohne [claimId].",
                    LineNumber: lineNo, ArtifactQuote: Trim(text), ClaimIds: [],
                    LedgerFacets: Empty, SuggestedAction: "Zeile mit Quelle belegen oder entfernen."));
                continue;
            }

            linesWithCitation++;
            var known = new List<SemanticLedgerEntry>();
            foreach (var id in ids)
            {
                citedIds.Add(id);
                if (!claims.TryGetValue(id, out var claim))
                {
                    // C2 — zitierte ID existiert nicht im consumable. Nicht auto-reparierbar -> HumanReview.
                    unknownIdCount++;
                    violations.Add(new ContractViolation(
                        ContractCodes.UnknownClaimId, ContractSeverity.Error, Repairable: false,
                        Message: $"Zitierte ID '{id}' ist nicht im consumable.json.",
                        LineNumber: lineNo, ArtifactQuote: Trim(text), ClaimIds: [id],
                        LedgerFacets: Empty, SuggestedAction: "ID korrigieren oder Aussage entfernen."));
                    continue;
                }

                known.Add(claim);

                // C4 — Claim ist für das requirements-Artefakt nicht verwertbar (not_applicable).
                var disp = Applicability(claim, artifactDisposition);
                if (string.Equals(disp, "not_applicable", StringComparison.OrdinalIgnoreCase))
                {
                    wrongDispCount++;
                    violations.Add(new ContractViolation(
                        ContractCodes.WrongDisposition, ContractSeverity.Error, Repairable: false,
                        Message: $"Claim '{id}' hat {artifactDisposition}-disposition=not_applicable und gehört nicht ins {artifactDisposition}-Artefakt.",
                        LineNumber: lineNo, ArtifactQuote: Trim(text), ClaimIds: [id],
                        LedgerFacets: Facets(claim, artifactDisposition), SuggestedAction: "Zeile entfernen oder Disposition prüfen."));
                }
            }

            // C3 — Heuristik (Warning, NICHT blockierend): harte Sprache trotz weicher Facette eines zitierten Claims.
            var soft = known.Any(c =>
                SoftStatus.Contains(c.Status) || SoftModality.Contains(c.Modality)
                || (c.TimeScope is not null && SoftTime.Contains(c.TimeScope)));
            if (soft && HardRx.IsMatch(text) && !SoftRx.IsMatch(text))
            {
                facetCandidates++;
                var facets = string.Join("; ", known.Select(c =>
                    $"{c.Id}: status={c.Status},modality={c.Modality},timeScope={c.TimeScope ?? "?"}"));
                violations.Add(new ContractViolation(
                    ContractCodes.FacetOverstated, ContractSeverity.Warning, Repairable: false,
                    Message: "Heuristik-Kandidat: harte Formulierung trotz weicher Facette. Bestätigung durch Critic (MC3) nötig.",
                    LineNumber: lineNo, ArtifactQuote: Trim(text), ClaimIds: known.Select(c => c.Id).ToList(),
                    LedgerFacets: new Dictionary<string, string> { ["candidates"] = facets },
                    SuggestedAction: "Formulierung an die Facette angleichen, falls Critic den Verstoß bestätigt."));
            }
        }

        // C5 — required-Claims, die von keiner Zeile zitiert werden (Coverage gegen den GESCHLOSSENEN Claim-Satz).
        // E-b (2026-07-08): C5 ist NICHT auto-reparierbar (Repairable:false). Der zeilenweise ContractRepair ist rein
        // SUBTRAKTIV (entfernt/entschärft geflaggte Zeilen). Eine fehlende Zeile HINZUZUFÜGEN ist ein additiver,
        // generativer Akt und gehört an den Menschen -> Decision landet über den Non-Repairable-Zweig bei HumanReview.
        // Das RepairItem bleibt als MENSCHEN-Hinweis (was ergänzen, mit welchen Facetten-Grenzen), wird aber von KEINEM
        // Auto-Repairer konsumiert. Damit lügt das Repairable-Flag nicht mehr über die tatsächliche Fähigkeit.
        var repairItems = new List<RepairItem>();
        var requiredClaims = ledger.Claims
            .Where(c => string.Equals(Applicability(c, artifactDisposition), "required", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var missingRequired = requiredClaims.Where(c => !citedIds.Contains(c.Id)).ToList();
        foreach (var c in missingRequired)
        {
            violations.Add(new ContractViolation(
                ContractCodes.RequiredClaimUnused, ContractSeverity.Error, Repairable: false,
                Message: $"required-Claim '{c.Id}' wird von keiner Zeile zitiert.",
                LineNumber: null, ArtifactQuote: null, ClaimIds: [c.Id],
                LedgerFacets: Facets(c, artifactDisposition), SuggestedAction: "Artefaktzeile für diesen Claim ergänzen (mit Source-ID)."));

            repairItems.Add(new RepairItem(
                Id: $"repair-{c.Id}", ViolationCode: ContractCodes.RequiredClaimUnused, LineNumber: null,
                CurrentText: string.Empty, ClaimIds: [c.Id],
                Instruction: "Ergänze eine Artefaktzeile für diesen Claim mit Source-ID; respektiere die Facetten (nicht verstärken).",
                AllowedFacetBounds: Facets(c, artifactDisposition),
                EvidenceQuotes: (c.Evidence ?? [])
                    .Select(e => e.Quote).Where(q => !string.IsNullOrWhiteSpace(q)).ToList()));
        }

        // Entscheidung (Contract.md §3): nur Error blockiert; Präzedenz Max -> Repair -> HumanReview.
        var errors = violations.Where(v => v.Severity == ContractSeverity.Error).ToList();
        var pass = errors.Count == 0;
        var decision =
            pass ? ContractDecision.Pass
            : iteration >= maxIterations ? ContractDecision.MaxIterationsReached
            : errors.Any(e => e.Repairable) ? ContractDecision.Repair
            : ContractDecision.HumanReview;

        var checks = new Dictionary<string, object>
        {
            ["requirementLines"] = lines.Count,
            ["citation"] = new { lines = lines.Count, cited = linesWithCitation, missing = lines.Count - linesWithCitation },
            ["knownId"] = new { citedDistinct = citedIds.Count, unknown = unknownIdCount },
            ["disposition"] = new { notApplicableCited = wrongDispCount },
            ["requiredCoverage"] = new { required = requiredClaims.Count, unused = missingRequired.Count },
            ["facetHeuristic"] = new { candidates = facetCandidates, note = "Warning-only; Bestätigung durch Critic (MC3)" }
        };

        return new ContractCheckReport(pass, iteration, decision, violations, repairItems, checks);
    }

    private static List<(int LineNumber, string Text, IReadOnlyList<string> Ids)> ParseRequirementLines(string markdown)
    {
        var result = new List<(int, string, IReadOnlyList<string>)>();
        var raw = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        for (var i = 0; i < raw.Length; i++)
        {
            var line = raw[i];
            if (!ReqLineRx.IsMatch(line)) continue;
            result.Add((i + 1, line.Trim(), ExtractIds(line)));
        }
        return result;
    }

    private static IReadOnlyList<string> ExtractIds(string line)
    {
        var ids = new List<string>();
        foreach (Match m in IdRx.Matches(line))
        {
            foreach (var tok in m.Groups[1].Value.Split(','))
            {
                var id = tok.Trim();
                if (id.Length == 0) continue;
                if (Regex.IsMatch(id, @"^\d+$")) continue; // reine Zahl = keine Claim-ID
                ids.Add(id);
            }
        }
        return ids;
    }

    private static string? Applicability(SemanticLedgerEntry c, string dispositionKey)
        => c.Disposition is not null && c.Disposition.TryGetValue(dispositionKey, out var d) ? d.Applicability : null;

    private static IReadOnlyDictionary<string, string> Facets(SemanticLedgerEntry c, string dispositionKey) => new Dictionary<string, string>
    {
        ["status"] = c.Status,
        ["modality"] = c.Modality,
        ["timeScope"] = c.TimeScope ?? "?",
        [dispositionKey] = Applicability(c, dispositionKey) ?? "?"
    };

    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();

    private static string Trim(string s) => s.Length <= 200 ? s : s[..200] + "…";
}

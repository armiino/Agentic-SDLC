using System.Text;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

// ── L3 CoverageSpec: der geteilte Abdeckungs-Rahmen (Analogon zu DerivationSpec) ──────────────────────────────────
// EINE Quelle für zwei Konsumenten (kein Drift): (1) der Coverage-Generator-Prompt rendert die Linsen via
// RenderGeneratorBlock() in den Platzhalter {{coverageLenses}}; (2) der LensCoverageGate (Schritt 3) rechnet Abdeckung
// mechanisch aus den gapCategory-Feldern gegen LensIds. Fachliche Begründung + Quellen-Mapping: docs/L3-CoverageSpec-
// Katalog.md (FURPS+ / ISO/IEC/IEEE 29148 / Volere). Additiv erweiterbar: neue Linse = neue Zeile, kein Mechanismus.

/// <summary>Eine Prüflinse. <see cref="Id"/> IST der <c>gapCategory</c>-Wert im Kandidatenvertrag → der Gate rechnet
/// deterministisch. <see cref="AddressedCriterion"/> ist (noch) nicht vom v1-Gate genutzt (Existenz ≥1), sondern die
/// Vorlage für die spätere optionale Adäquanz-Schicht. <see cref="Mandatory"/> = darf nicht lautlos leer bleiben.</summary>
public sealed record CoverageLens(
    string Id,                    // = gapCategory-Wert, kebab-case (z. B. "security-misuse")
    string Name,                  // Anzeigename
    string GeneratorDescription,  // was der Generator je Linse betrachten soll (Prompt-facing)
    string RepairHint,            // Hinweis bei leerer Linse (Repair-Pass, Schritt 4)
    string AddressedCriterion,    // "adressiert, wenn …" — für die spätere Adäquanz-Schicht
    bool Mandatory = false);      // Pflicht-Linse: leer ⇒ expliziter N/A-Grund nötig, nicht lautlos

/// <summary>Der Abdeckungs-Rahmen = geordnete Linsen-Menge. Wird injiziert (nicht hartkodiert im Prompt), damit der
/// Checker-Knoten wiederverwendbar bleibt und Prompt+Gate an dieselbe Quelle gebunden sind.</summary>
public sealed record CoverageSpec(string Id, string Name, IReadOnlyList<CoverageLens> Lenses)
{
    /// <summary>Alle Linsen-Ids (= erwartbare gapCategory-Werte). Der Gate prüft Kandidaten-gapCategory hiergegen.</summary>
    public IReadOnlySet<string> LensIds => Lenses.Select(l => l.Id).ToHashSet(StringComparer.Ordinal);

    /// <summary>Pflicht-Linsen — leer erlaubt nur mit explizitem N/A-Grund (Gate/Repair, Schritt 3/4).</summary>
    public IReadOnlySet<string> MandatoryLensIds => Lenses.Where(l => l.Mandatory).Select(l => l.Id).ToHashSet(StringComparer.Ordinal);

    public bool TryGetLens(string id, out CoverageLens lens)
    {
        lens = Lenses.FirstOrDefault(l => string.Equals(l.Id, id, StringComparison.Ordinal))!;
        return lens is not null;
    }

    /// <summary>Rendert die Linsen-Liste für den Generator-Prompt-Platzhalter <c>{{coverageLenses}}</c>. Format bewusst
    /// wie die bisherige Inline-Liste (Bullet + `id` + Name: Beschreibung), damit v1→v2 inhaltlich anschließt.</summary>
    public string RenderGeneratorBlock()
    {
        var sb = new StringBuilder();
        foreach (var l in Lenses)
        {
            sb.Append("* `").Append(l.Id).Append("` — ").Append(l.Name).Append(": ").Append(l.GeneratorDescription);
            if (l.Mandatory) sb.Append(" **(Pflicht-Linse: nicht lautlos leer lassen — sonst explizit als N/A mit Grund kennzeichnen)**");
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>Kompaktes Label-Vokabular (nur `id` + Name) für den measure-Prompt-Platzhalter <c>{{coverageLabels}}</c>.
    /// Bewusst OHNE Beschreibungen/Pflicht-Marker/„absuchen"-Rahmung → dient nur der Klassifikation, minimiert das
    /// Priming (Reaktivität), das der sichtbare Voll-Katalog erzeugt.</summary>
    public string RenderLabelVocabulary()
        => string.Join(Environment.NewLine, Lenses.Select(l => $"* `{l.Id}` — {l.Name}"));

    /// <summary>Repair-Hinweise für die (leeren) Linsen im Repair-Pass (Schritt 4): je Linse `id` + RepairHint.</summary>
    public string RenderRepairHints(IReadOnlyList<CoverageLens> missing)
        => string.Join(Environment.NewLine, missing.Select(l => $"* `{l.Id}` — {l.RepairHint}"));
}

/// <summary>Registry der Abdeckungs-Rahmen. Code-Default, später config-wählbar (l3.coverageSpec). Additiv: neuer
/// Rahmen = neuer Eintrag (z. B. ein schlankerer Sprint-Delta-Rahmen), ohne Mechanismus-Änderung.</summary>
public static class CoverageRegistry
{
    // "early-phase" = 12 Linsen für die frühe SDLC-Planung. Katalog + Quellen: docs/L3-CoverageSpec-Katalog.md.
    public static readonly IReadOnlyDictionary<string, CoverageSpec> Specs =
        new Dictionary<string, CoverageSpec>(StringComparer.OrdinalIgnoreCase)
        {
            ["early-phase"] = new("early-phase", "Frühe SDLC-Planung (FURPS+ / ISO 29148 / Volere)",
            [
                new("business-goals", "Fachliche Ziele, Mission & Scope",
                    "Kernzweck, Nutzenversprechen und Abgrenzung (was gehört dazu, was ausdrücklich NICHT); die fachlichen Kernabläufe.",
                    "Ziele/Scope-Abgrenzung nicht adressiert — was leistet das System im Kern, und wo ist die Grenze?",
                    "≥1 Kandidat benennt ein konkretes fachliches Ziel ODER eine Scope-Grenze (keine Floskel)."),

                new("actors-permissions", "Stakeholder, Akteure, Rollen & Berechtigungen",
                    "Wer nutzt/betrifft das System (Rollen, Stakeholder, Dritte wie Angehörige); wer darf was lesen/ändern/weitergeben.",
                    "Akteure/Rollen/Berechtigungen unklar — welche Stakeholder gibt es, und wer darf was?",
                    "≥1 Kandidat benennt eine Rolle/Stakeholder ODER eine rollenbezogene Zugriffsregel."),

                new("input-validation", "Eingaben, Validierung & Fehler-/Leerzustände",
                    "Pflichtangaben, Wertebereiche, Formatregeln; Verhalten bei fehlerhaften, unvollständigen oder leeren Daten.",
                    "Eingabe-/Validierungsregeln fehlen — was passiert bei ungültigen oder fehlenden Eingaben?",
                    "≥1 Kandidat benennt eine Validierungs-/Fehler-/Leerzustandsregel."),

                new("state-transitions", "Zustände, Übergänge & Nebenläufigkeit",
                    "Objekt-/Vorgangszustände, erlaubte Übergänge, Nebenwirkungen, gleichzeitige/zeitversetzte Änderungen (Konflikte, Synchronisation).",
                    "Zustände/Übergänge/Nebenläufigkeit unklar — welche Zustände gibt es, und wie werden Konflikte behandelt?",
                    "≥1 Kandidat benennt einen Zustandsübergang, eine Nebenwirkung oder ein Konflikt-/Sync-Verhalten."),

                new("data-lifecycle", "Daten, Datenschutz, Aufbewahrung & Löschung",
                    "Welche (sensiblen) Daten entstehen; Speicherdauer; Löschung, Anonymisierung, Export, Archivierung.",
                    "Datenlebenszyklus fehlt — wie werden Daten aufbewahrt, gelöscht, anonymisiert oder exportiert?",
                    "≥1 Kandidat benennt eine Aufbewahrungs-/Lösch-/Export-/Anonymisierungsregel."),

                new("security-misuse", "Sicherheit & Missbrauchsfälle",
                    "Plausible Missbrauchs-/Bedrohungsszenarien, Schutz vor unbefugtem Zugriff, Datenschutzverletzungen, Manipulation "
                    + "(adversarielle Sicht; die Zugriffs-*rechte* liegen in actors-permissions).",
                    "Sicherheit/Missbrauch nicht betrachtet — welche Bedrohungs-/Missbrauchsfälle sind plausibel, und wie schützt das System sensible Daten?",
                    "≥1 Kandidat benennt ein Bedrohungs-/Missbrauchsszenario ODER eine Schutzmaßnahme.",
                    Mandatory: true),

                new("external-integration", "Externe Systeme, Schnittstellen & Integrationen",
                    "Mit welchen Systemen/Datenquellen wird zusammengearbeitet; lesend/bidirektional; Datenformate, Medienbrüche.",
                    "Integrationen unklar — mit welchen Systemen/Datenquellen arbeitet die Lösung zusammen und wie?",
                    "≥1 Kandidat benennt ein externes System, eine Schnittstelle oder eine Integrationsanforderung."),

                new("resilience-operations", "Ausfall, Wiederherstellung, Offline & Betrieb",
                    "Verhalten bei Störungen (Cloud/Verbindung), Offline-Fähigkeit, Wiederherstellung, Betrieb/Wartung, Monitoring.",
                    "Ausfall-/Betriebsverhalten fehlt — was passiert bei Störungen, und welche Aufgaben bleiben möglich?",
                    "≥1 Kandidat benennt ein Ausfall-/Offline-/Wiederherstellungs-/Betriebsverhalten."),

                new("quality-attributes", "Performance, Verfügbarkeit & Skalierbarkeit",
                    "Mengengerüst (Nutzer/Daten/Einrichtungen), Antwortzeiten, Verfügbarkeit, Skalierungsziele.",
                    "Qualitätsziele fehlen — welche Größenordnung/Performance/Verfügbarkeit muss erreicht werden?",
                    "≥1 Kandidat benennt ein messbares Qualitäts-/Mengen-/Performanceziel (oder den Klärungsbedarf dazu)."),

                new("usage-context", "Geräte-, Zugriffs- & Nutzungskontext, Bedienbarkeit",
                    "Einsatzbedingungen (mobil, Schichtbetrieb, Umgebung), Bedienbarkeit im realen Arbeitskontext, Barrierefreiheit.",
                    "Nutzungskontext fehlt — unter welchen realen Bedingungen muss die Lösung zuverlässig bedienbar sein?",
                    "≥1 Kandidat benennt eine Einsatzbedingung oder ein Bedienbarkeits-/Zugänglichkeitskriterium."),

                new("constraints-compliance", "Rand-/Rahmenbedingungen, Recht, Regulatorik & Standards",
                    "Rechtliche/regulatorische Vorgaben (z. B. DSGVO, Pflegevorschriften), einzuhaltende Standards, "
                    + "technische/organisatorische Zwänge (Plattform, Budget, Fristen).",
                    "Rahmenbedingungen/Compliance nicht betrachtet — welche rechtlichen, regulatorischen oder technischen Zwänge gelten?",
                    "≥1 Kandidat benennt eine rechtliche/regulatorische/standard-/plattformbezogene Randbedingung."),

                new("open-decisions", "Ungeklärte Annahmen, offene Entscheidungen & Abhängigkeiten",
                    "Was ist noch nicht entschieden; unbelegte Annahmen; externe Abhängigkeiten — als Requirement + requiresHumanDecision=true, "
                    + "keine unbelegte Muss-Anforderung.",
                    "Offene Entscheidungen/Annahmen nicht ausgewiesen — welche Klärungen stehen noch aus?",
                    "≥1 Kandidat benennt eine offene Entscheidung, Annahme oder Abhängigkeit (mit Klärungsbedarf)."),
            ]),
        };

    public static CoverageSpec Default => Specs["early-phase"];

    public static bool TryGet(string id, out CoverageSpec spec) => Specs.TryGetValue(id, out spec!);
}

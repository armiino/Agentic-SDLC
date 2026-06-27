using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Verdikt einer einzelnen Prüfeinheit (DISK-14 Per-Item-Klassifikation).</summary>
public sealed record UnitVerdict(int Index, string Verdict, string Reason);

/// <summary>
/// DISK-14 Phase 2: begrenzte Per-Item-KLASSIFIKATION (statt offener Fehler-Generierung). Pro Einheit
/// genau ein Verdikt aus <c>grounded | overstated | fabricated | not_a_claim</c>, gechunkt (DISK-9-Lektion).
/// </summary>
/// <remarks>
/// BEWUSST ISOLIERT + REVERSIBEL: Prompts sind hier als Konstanten eingebettet (kein externer Prompt-
/// Ordner) → Verwerfen = `PerItem/`-Ordner löschen + die `classify-units`-Dispatch-Zeile. Kein Eingriff
/// in Evaluator/Jury. `not_a_claim` ist das Netz gegen Parser-Unschärfe (Metadaten/Mapping → zählt nicht).
/// </remarks>
public sealed class UnitClassifier
{
    public const int ChunkSize = 12;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string BasePrompt = """
        Du pruefst Einheiten eines SDLC-Artefakts gegen das originale Stakeholder-Transkript
        (Ground Truth). Du bekommst das VOLLSTAENDIGE Transkript und eine NUMMERIERTE Liste von
        Artefakt-Einheiten. Klassifiziere JEDE Einheit mit GENAU EINEM Verdikt:

        grounded    = Der Sachverhalt/Konflikt/die Unsicherheit wird im Transkript besprochen (auch
          sinngemaess/paraphrasiert). Unsicherheits-/Offenheitssprache ("unklar", "koennte", "offen",
          "Annahme", "noch zu klaeren") ist KEIN Fehler, solange das Thema im Transkript vorkommt und
          die Offenheit korrekt markiert ist.
        overstated  = Das Thema kommt im Transkript vor, ABER die Einheit stellt etwas als
          entschieden/gesetzt dar, das offen ist, ODER nennt eine konkrete Zahl/Technologie/Frist als
          gesetzt, die das Transkript so nicht hergibt — OHNE sie als Annahme/offen zu kennzeichnen.
        fabricated  = Der Sachverhalt kommt im Transkript UEBERHAUPT NICHT vor (frei erfunden) oder
          widerspricht ihm direkt.
        not_a_claim = Die Einheit ist kein fachlicher Claim: Metadaten, Traceability-/Owner-Mapping
          ("Thema | Stakeholdername"), reine Ueberschrift/Label, Aufzaehlung ohne Aussage. Zaehlt NICHT.

        [PROFIL]

        Antworte ausschliesslich mit JSON:
        { "results": [ { "index": 0, "verdict": "grounded|overstated|fabricated|not_a_claim", "reason": "kurz" } ] }
        Genau ein Ergebnis pro Einheit (Index = die Nummer aus der Liste). Kein Text ausserhalb des JSON.
        """;

    // Artefakttypische Bewertungs-Schwerpunkte (generisch, kein Themen-Katalog).
    private static readonly IReadOnlyDictionary<string, string> Profiles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["requirements"] = "PROFIL requirements: Behandle konkrete Zusagen (SLA/Uptime, Zahlen, "
                + "Technologien, Fristen) streng. Ohne Transkript-Grundlage und ohne Annahme-Markierung "
                + "→ fabricated; mit Annahme-/Offen-Markierung, aber im Transkript nicht besprochen → overstated.",
            ["risks"] = "PROFIL risks: Ein Risiko DARF Unsicherheit ausdruecken. Ein im Transkript "
                + "besprochener Konflikt/eine Unsicherheit als Risiko = grounded, auch mit ergaenztem "
                + "Trigger/Auswirkung/Klaerungsbedarf. Nur ein frei erfundenes Risiko = fabricated.",
            ["architecture"] = "PROFIL architecture: Pruefe technische Festlegungen (Protokolle, Services, "
                + "Crypto/KMS, konkrete Zahlen) hart gegen Transkript-Evidenz. Erfundene Konkretisierung → "
                + "fabricated; als 'geplant/optional/MVP-minimal' markierte Offenheit → grounded.",
            ["open-questions"] = "PROFIL open-questions: Dies ist ein Dokument OFFENER FRAGEN. Eine offene "
                + "Frage DARF konkrete Beispiel-Optionen, -Parameter, -Werte, -Fristen oder -Technologien "
                + "nennen, um die Frage zu praezisieren (z. B. 'Azure AD oder Google?', "
                + "'Loeschintervall z. B. 30 Tage?', 'Token-Lebensdauer?', 'Pagination/Rate-Limiting wie?'). "
                + "Solche Beispiele in einer Frage sind NICHT fabricated und NICHT overstated — die Frage "
                + "TRIFFT keine Festlegung, sie fragt nur. Verdikt fuer eine offene Frage: grounded, wenn ihr "
                + "THEMA im Transkript besprochen wird (auch nur allgemein/angerissen); fabricated NUR, wenn "
                + "das Thema der Frage im Transkript UEBERHAUPT NICHT vorkommt. overstated ist bei Fragen "
                + "praktisch nicht anwendbar.",
            ["generic"] = "PROFIL generic: Bewerte sachlich gegen das Transkript."
        };

    private static readonly IReadOnlyDictionary<string, string> ProfilesV22 =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["requirements"] = "PROFIL requirements: Requirements sind verbindliche Aussagen. Pruefe deshalb "
                + "Modalitaet, Status und Scope streng. Wenn das Transkript ein Thema nur diskutiert/offen laesst, "
                + "die Unit aber 'muss', 'darf nur', 'ausschliesslich', 'im MVP' oder 'gesichert' behauptet, ist "
                + "das overstated. Zulässige Verdichtungen sind grounded, solange keine neue Verpflichtung, kein "
                + "neuer Scope und keine neue technische Praezisierung entsteht.",
            ["risks"] = "PROFIL risks: Risiken duerfen Auswirkungen, Trigger und Gegenmassnahmen formulieren, "
                + "die aus einem besprochenen Risiko fachlich naheliegend folgen. Markiere solche Risiko-"
                + "Auswirkungen NICHT als fabricated, nur weil sie nicht woertlich im Transkript stehen. Streng "
                + "bleiben bei konkreten neuen Technologien/Methoden, Zahlen, finalen Entscheidungen oder "
                + "Massnahmen, die als beschlossen dargestellt werden. Eine Gegenmassnahme ist grounded, wenn "
                + "der zugrunde liegende Konflikt/Klaerungsbedarf belegt ist und die Massnahme generisch bleibt.",
            ["architecture"] = "PROFIL architecture: Architektur-Claims sind technische Festlegungen. Pruefe "
                + "Modalitaet, Status, Scope und Zeitbezug streng. Wenn das Transkript ein Thema als offen, "
                + "kosten-/machbarkeitsabhaengig oder zu klaeren beschreibt, die Unit aber eine gesetzte "
                + "Architekturentscheidung behauptet, ist das overstated. Besonders streng bei 'nur', "
                + "'ausschliesslich', 'gesichert', 'MVP', konkreten Services/Protokollen, Hosting, Backup, "
                + "Datenresidenz und Integrationsgrenzen.",
            ["open-questions"] = "PROFIL open-questions: Offene Fragen duerfen Beispieloptionen, Parameter oder "
                + "Technologien nennen, um die Frage zu praezisieren. Grounded, wenn das Thema im Transkript "
                + "besprochen ist und die Unit als Frage/offen formuliert bleibt. Overstated nur, wenn die Frage "
                + "in Wahrheit eine Festlegung behauptet oder eine Option als gesetzt darstellt.",
            ["generic"] = "PROFIL generic: Bewerte sachlich gegen das Transkript. Thema plus Claim-Staerke muessen "
                + "durch die Evidence getragen sein."
        };

    private static readonly IReadOnlyDictionary<string, string> ProfilesV23 =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["requirements"] = "PROFIL requirements: Requirements sind testbare Synthesen aus dem Transkript, "
                + "keine woertlichen Mitschriften. Grounded ist eine Unit, wenn Inhalt, Scope und Zielrichtung "
                + "klar belegt sind, auch wenn sie als Requirement testbar formuliert wird. Markiere nur dann "
                + "overstated, wenn die Unit eine harte neue Verpflichtung oder einen finalen Ausschluss behauptet "
                + "('darf nur', 'ausschliesslich', 'muss vollstaendig', 'kein X im MVP', 'gesichert'), obwohl das "
                + "Transkript offen, optional, widerspruechlich oder nur als Zwischenloesung diskutiert. "
                + "MVP-Schnittlisten duerfen als Requirements formuliert werden, wenn sie im Transkript klar "
                + "priorisiert wurden.",
            ["risks"] = "PROFIL risks: Risiken duerfen Auswirkungen, Trigger und Gegenmassnahmen formulieren, "
                + "die aus einem besprochenen Risiko fachlich naheliegend folgen. Markiere solche Risiko-"
                + "Auswirkungen NICHT als fabricated, nur weil sie nicht woertlich im Transkript stehen. Streng "
                + "bleiben bei konkreten neuen Technologien/Methoden, Zahlen, finalen Entscheidungen oder "
                + "Massnahmen, die als beschlossen dargestellt werden. Eine Gegenmassnahme ist grounded, wenn "
                + "der zugrunde liegende Konflikt/Klaerungsbedarf belegt ist und die Massnahme generisch bleibt.",
            ["architecture"] = "PROFIL architecture: Architekturartefakte duerfen aus besprochenen Komponenten "
                + "einen strukturierten Ueberblick bilden. Grounded ist ein Baustein, wenn das Thema und die "
                + "technische Zielrichtung belegt sind und keine konkrete neue Garantie/Festlegung entsteht. "
                + "Overstated ist eine Unit nur bei harter Verschiebung: offen -> entschieden, moeglich -> "
                + "verpflichtend, spaeter -> MVP, Teilscope -> Gesamtscope, 'EU/DSGVO diskutiert' -> "
                + "'ausschliesslich/nur/gesichert EU', oder konkrete Services/Protokolle/Provider/Eigenschaften "
                + "ohne Beleg. Ueberblickssaetze und offene Architekturpunkte duerfen knapper und technischer "
                + "formuliert sein, solange die Offenheit erhalten bleibt.",
            ["open-questions"] = "PROFIL open-questions: Offene Fragen duerfen Beispieloptionen, Parameter oder "
                + "Technologien nennen, um die Frage zu praezisieren. Grounded, wenn das Thema im Transkript "
                + "besprochen ist und die Unit als Frage/offen formuliert bleibt. Overstated nur, wenn die Frage "
                + "in Wahrheit eine Festlegung behauptet oder eine Option als gesetzt darstellt.",
            ["generic"] = "PROFIL generic: Bewerte sachlich gegen das Transkript. Thema plus Claim-Staerke muessen "
                + "durch die Evidence getragen sein."
        };

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "results": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "index": { "type": "integer" },
                  "verdict": { "type": "string" },
                  "reason": { "type": "string" }
                },
                "required": ["index", "verdict", "reason"],
                "additionalProperties": false
              }
            }
          },
          "required": ["results"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "peritem_classification",
        "Per-Item-Klassifikation von SDLC-Artefakt-Einheiten gegen das Transkript.");

    // --- v2: Evidenz-gebunden, Reason-VOR-Verdikt, themen-tolerantes Kriterium (B40-Befund: v1 über-flaggt
    //     inferenzielle/offene Artefakte, weil es „wörtlich im Transkript?" prüft statt „Thema besprochen?").
    //     v1 bleibt UNVERÄNDERT; Auswahl über Konstruktor/Flag. ---
    private const string BasePromptV2 = """
        Du pruefst Einheiten eines SDLC-Artefakts gegen das originale Stakeholder-Transkript (Ground Truth).
        Du bekommst das VOLLSTAENDIGE Transkript und eine NUMMERIERTE Liste von Artefakt-Einheiten.

        Arbeite PRO Einheit in GENAU dieser Reihenfolge (erst belegen + denken, DANN urteilen):
        1) evidence: Suche im Transkript die KONKRETE Stelle (woertliches Teilzitat einer Sprecher-Aussage),
           die das THEMA der Einheit stuetzt ODER ihr widerspricht. Findest du KEINE Stelle zum Thema:
           schreibe exakt "KEIN BELEG".
        2) reasoning: kurze Begruendung auf Basis der evidence.
        3) verdict: ERST JETZT entscheiden.

        VERDIKTE:
        grounded    = Das THEMA der Einheit wird im Transkript besprochen (auch sinngemaess/paraphrasiert/
          nur angerissen). WICHTIG: Eine Anforderung, ein Risiko, eine Gegenmassnahme oder eine OFFENE FRAGE
          DARF ueber den Wortlaut hinausgehen (extrapolieren, konkretisieren, eine Massnahme/Frage
          formulieren), solange ihr Thema im Transkript vorkommt. Unsicherheits-/Offenheitssprache ist KEIN Fehler.
        overstated  = Das Thema kommt vor, ABER die Einheit stellt etwas als entschieden/gesetzt dar (konkrete
          Zahl/Technologie/Frist), das das Transkript so nicht hergibt — OHNE Annahme-/Offen-Markierung.
        fabricated  = NUR wenn das THEMA im Transkript UEBERHAUPT NICHT vorkommt (evidence = "KEIN BELEG")
          oder die Einheit dem Transkript direkt widerspricht.
        not_a_claim = kein fachlicher Claim (Metadaten, reine Ueberschrift/Label, Owner-Mapping). Zaehlt NICHT.

        ENTSCHEIDUNGSREGEL: evidence-Stelle zum Thema gefunden -> grounded (oder overstated, wenn als gesetzt
        dargestellt). NUR bei evidence = "KEIN BELEG" -> fabricated. Im Zweifel, wenn das Thema irgendwo
        vorkommt: grounded.

        [PROFIL]

        Antworte ausschliesslich mit JSON:
        { "results": [ { "index": 0, "evidence": "...", "reasoning": "...",
          "verdict": "grounded|overstated|fabricated|not_a_claim" } ] }
        Genau ein Ergebnis pro Einheit (Index = die Nummer aus der Liste). Kein Text ausserhalb des JSON.
        """;

    private const string SchemaJsonV2 = """
        {
          "type": "object",
          "properties": {
            "results": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "index": { "type": "integer" },
                  "evidence": { "type": "string" },
                  "reasoning": { "type": "string" },
                  "verdict": { "type": "string" }
                },
                "required": ["index", "evidence", "reasoning", "verdict"],
                "additionalProperties": false
              }
            }
          },
          "required": ["results"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormatV2 = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJsonV2).RootElement.Clone(),
        "peritem_classification_v2",
        "Per-Item-Klassifikation (Evidenz-gebunden, Reason-vor-Verdikt).");

    private const string BasePromptV22 = """
        Du pruefst Einheiten eines SDLC-Artefakts gegen das originale Stakeholder-Transkript (Ground Truth).
        Du bekommst das VOLLSTAENDIGE Transkript und eine NUMMERIERTE Liste von Artefakt-Einheiten.

        Arbeite PRO Einheit in GENAU dieser Reihenfolge:
        1) evidence: Suche im Transkript konkrete Teilzitate, die das THEMA stuetzen, einschraenken oder
           widersprechen. Findest du KEINE Stelle zum Thema: schreibe exakt "KEIN BELEG".
        2) reasoning: Pruefe anhand der Evidence UND des artefaktspezifischen Profils, ob nicht nur das Thema,
           sondern auch die Claim-Staerke getragen ist: Modalitaet, Status, Scope, Zeitbezug, Entitaeten,
           Zahlen, Technologien.
        3) verdict: ERST JETZT entscheiden.

        VERDIKTE:
        grounded    = Das Thema ist belegt und die Claim-Staerke passt zum Artefakttyp. Paraphrasen,
          Verdichtungen und fachlich naheliegende Synthesen sind erlaubt.
        overstated  = Das Thema kommt vor, aber die Unit macht die Aussage staerker/finaler/breiter als das
          Transkript: offen -> entschieden, moeglich -> verpflichtend, diskutiert -> beschlossen,
          spaeter -> MVP, Teilscope -> Gesamtscope, oder Daten/Hosting/Backup/Technologie gesicherter als belegt.
        fabricated  = Das Thema kommt im Transkript UEBERHAUPT NICHT vor (evidence = "KEIN BELEG") oder die
          Unit widerspricht dem Transkript direkt.
        not_a_claim = kein fachlicher Claim (Metadaten, reine Ueberschrift/Label, Owner-Mapping). Zaehlt NICHT.

        WICHTIG:
        - Thema gefunden reicht bei requirements/architecture nicht automatisch fuer grounded; dort muss die
          konkrete Verpflichtung/Festlegung getragen sein.
        - Bei risks duerfen Auswirkungen und generische Gegenmassnahmen weiter gehen als der Wortlaut, solange
          der zugrunde liegende Konflikt belegt ist und keine konkrete unbelegte Technik/Festlegung erfunden wird.

        [PROFIL]

        Antworte ausschliesslich mit JSON:
        { "results": [ { "index": 0, "evidence": "...", "reasoning": "...",
          "verdict": "grounded|overstated|fabricated|not_a_claim" } ] }
        Genau ein Ergebnis pro Einheit (Index = die Nummer aus der Liste). Kein Text ausserhalb des JSON.
        """;

    private const string BasePromptV23 = """
        Du pruefst Einheiten eines SDLC-Artefakts gegen das originale Stakeholder-Transkript (Ground Truth).
        Du bekommst das VOLLSTAENDIGE Transkript und eine NUMMERIERTE Liste von Artefakt-Einheiten.

        Arbeite PRO Einheit in GENAU dieser Reihenfolge:
        1) evidence: Suche konkrete Teilzitate, die das Thema stuetzen, einschraenken oder widersprechen.
           Findest du KEINE Stelle zum Thema: schreibe exakt "KEIN BELEG".
        2) reasoning: Pruefe anhand der Evidence UND des artefaktspezifischen Profils, ob die Unit eine
           legitime Synthese ist oder eine harte Bedeutungsverschiebung enthaelt.
        3) verdict: ERST JETZT entscheiden.

        VERDIKTE:
        grounded    = Thema und relevante Claim-Staerke sind belegt ODER die Unit ist eine legitime
          artefakttypische Synthese/Strukturierung ohne neue harte Festlegung. Eine gute Zusammenfassung muss
          nicht woertlich im Transkript stehen.
        overstated  = Thema kommt vor, aber die Unit verschiebt die Bedeutung hart: offen -> entschieden,
          optional -> verpflichtend, diskutiert -> final ausgeschlossen/beschlossen, spaeter -> MVP,
          Teilscope -> Gesamtscope, oder eine Garantie/Exklusivitaet ("nur", "ausschliesslich", "gesichert")
          wird behauptet, obwohl die Evidence sie nicht traegt.
        fabricated  = Thema kommt im Transkript UEBERHAUPT NICHT vor (evidence = "KEIN BELEG") oder die Unit
          widerspricht dem Transkript direkt.
        not_a_claim = kein fachlicher Claim (Metadaten, reine Ueberschrift/Label, Owner-Mapping). Zaehlt NICHT.

        WICHTIG:
        - Nicht jede fehlende Woertlichkeit ist overstated. Overstated braucht eine erkennbare harte
          Bedeutungsverschiebung.
        - Bei requirements/architecture: harte Exklusivitaeten, Garantien, finale Ausschluesse und konkrete
          technische Festlegungen streng pruefen.
        - Bei risks: Risikoauswirkungen und generische Gegenmassnahmen sind erlaubt, wenn der Konflikt belegt ist.

        [PROFIL]

        Antworte ausschliesslich mit JSON:
        { "results": [ { "index": 0, "evidence": "...", "reasoning": "...",
          "verdict": "grounded|overstated|fabricated|not_a_claim" } ] }
        Genau ein Ergebnis pro Einheit (Index = die Nummer aus der Liste). Kein Text ausserhalb des JSON.
        """;

    // --- v2.1: v2 + explizite Claim-Staerke-Pruefung. Ziel: v2s FP-Reduktion behalten, aber
    //     Over-Claims wie "nur/ausschliesslich", "entschieden", "MVP" nicht allein wegen Themenbeleg grounden. ---
    private const string BasePromptV21 = """
        Du pruefst Einheiten eines SDLC-Artefakts gegen das originale Stakeholder-Transkript (Ground Truth).
        Du bekommst das VOLLSTAENDIGE Transkript und eine NUMMERIERTE Liste von Artefakt-Einheiten.

        Arbeite PRO Einheit in GENAU dieser Reihenfolge (erst belegen + Claim-Staerke pruefen, DANN urteilen):
        1) evidence: Suche im Transkript die KONKRETEN Stellen (woertliche Teilzitate), die das THEMA der
           Einheit stuetzen, einschraenken oder ihr widersprechen. Findest du KEINE Stelle zum Thema:
           schreibe exakt "KEIN BELEG".
        2) reasoning: Begruende kurz, ob die Evidence nicht nur das Thema, sondern auch die STAERKE des Claims
           traegt. Pruefe explizit:
           - Modalitaet: muss/darf nur/ausschliesslich vs. soll/kann/moeglich/offen.
           - Status: entschieden/geplant/gesetzt vs. diskutiert/offen/unklar/zu klaeren.
           - Scope: MVP vs. spaeter; Hosting vs. Backup; Pilot vs. Produktivbetrieb; einzelne Daten vs. alle Daten.
           - Zeitbezug: jetzt/MVP vs. spaeter/noch zu klaeren.
           - Entitaeten/Zahlen/Technologien: werden konkrete Namen, Zahlen, Fristen oder Eigenschaften hinzugefuegt?
        3) verdict: ERST JETZT entscheiden.

        VERDIKTE:
        grounded    = Das Thema ist belegt UND Modalitaet, Status, Scope und Zeitbezug bleiben erhalten.
          Zulässig sind Paraphrasen, Verdichtungen, fachlich naheliegende Risiko-Auswirkungen und
          Gegenmassnahmen, wenn sie als Auswirkung/Klaerungsbedarf/Massnahme formuliert sind und keine
          neue harte Festlegung behaupten.
        overstated  = Das Thema kommt vor, ABER die Einheit macht die Aussage staerker/finaler/breiter als
          das Transkript, z. B. offen -> entschieden, moeglich -> verpflichtend, diskutiert -> beschlossen,
          spaeter -> MVP, Hosting -> Hosting+Backup, "DSGVO/EU diskutiert" -> "ausschliesslich/nur EU",
          oder "zu klaeren" -> "gesichert".
        fabricated  = Das Thema kommt im Transkript UEBERHAUPT NICHT vor (evidence = "KEIN BELEG") oder
          die Einheit widerspricht dem Transkript direkt.
        not_a_claim = kein fachlicher Claim (Metadaten, reine Ueberschrift/Label, Owner-Mapping). Zaehlt NICHT.

        WICHTIGE REGEL:
        Eine Evidence-Stelle zum Thema reicht NICHT automatisch fuer grounded. Wenn die Evidence nur das Thema
        traegt, aber nicht die Claim-Staerke (z. B. "nur", "ausschliesslich", "muss", "entschieden",
        "gesichert", "im MVP"), dann ist das verdict overstated.

        [PROFIL]

        Antworte ausschliesslich mit JSON:
        { "results": [ { "index": 0, "evidence": "...", "reasoning": "...",
          "verdict": "grounded|overstated|fabricated|not_a_claim" } ] }
        Genau ein Ergebnis pro Einheit (Index = die Nummer aus der Liste). Kein Text ausserhalb des JSON.
        """;

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly string _promptVersion;

    public UnitClassifier(IChatClient client, bool structuredOutput = true, string promptVersion = "v1")
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _promptVersion = NormalizePromptVersion(promptVersion);
    }

    /// <summary>Gewicht je Verdikt fuer den GroundingScore.</summary>
    public static int Weight(string verdict) => verdict switch
    {
        "fabricated" => 2,
        "overstated" => 1,
        _ => 0 // grounded / not_a_claim
    };

    public async Task<IReadOnlyList<UnitVerdict>> ClassifyAsync(
        string transcript, string artifactType, IReadOnlyList<ArtifactUnit> units, CancellationToken ct)
    {
        var basePrompt = _promptVersion switch
        {
            "v2" => BasePromptV2,
            "v2.1" => BasePromptV21,
            "v2.2" => BasePromptV22,
            "v2.3" => BasePromptV23,
            _ => BasePrompt
        };
        var system = basePrompt.Replace("[PROFIL]", ProfileFor(artifactType, _promptVersion));
        var verdicts = new Dictionary<int, UnitVerdict>();

        for (var offset = 0; offset < units.Count; offset += ChunkSize)
        {
            var chunk = units.Skip(offset).Take(ChunkSize).ToList();
            await ClassifyChunkAsync(system, transcript, chunk, verdicts, ct).ConfigureAwait(false);
        }

        // Nicht beantwortete Einheiten (sollten bei ChunkSize=12 nicht vorkommen) -> sichtbar als
        // 'unclassified' (NICHT still als grounded kaschieren).
        foreach (var u in units)
            if (!verdicts.ContainsKey(u.Index))
                verdicts[u.Index] = new UnitVerdict(u.Index, "unclassified", "kein Verdikt erhalten");

        return units.Select(u => verdicts[u.Index]).ToList();
    }

    private async Task ClassifyChunkAsync(
        string system, string transcript, List<ArtifactUnit> chunk,
        Dictionary<int, UnitVerdict> result, CancellationToken ct)
    {
        var listing = string.Join("\n", chunk.Select((u, k) => $"{k}: {u.Text}"));
        var messages = new[]
        {
            new ChatMessage(ChatRole.System, system),
            new ChatMessage(ChatRole.User, $"TRANSKRIPT:\n{transcript}\n\nARTEFAKT-EINHEITEN:\n{listing}")
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput)
            options.ResponseFormat = _promptVersion is "v2" or "v2.1" or "v2.2" or "v2.3" ? ResponseFormatV2 : ResponseFormat;

        var response = await _client.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        var json = ExtractJson(response.Text);
        if (json is null) return;

        try
        {
            if (_promptVersion is "v2" or "v2.1" or "v2.2" or "v2.3")
            {
                var parsed = JsonSerializer.Deserialize<EnvelopeV2>(json, Json);
                if (parsed?.Results is null) return;
                foreach (var r in parsed.Results)
                    if (r.Index >= 0 && r.Index < chunk.Count)
                    {
                        // Evidenz + Begründung in den Reason-String falten (Output-Typ bleibt stabil).
                        var reason = $"[{(string.IsNullOrWhiteSpace(r.Evidence) ? "—" : r.Evidence)}] {r.Reasoning}".Trim();
                        result[chunk[r.Index].Index] = new UnitVerdict(chunk[r.Index].Index, Normalize(r.Verdict), reason);
                    }
            }
            else
            {
                var parsed = JsonSerializer.Deserialize<Envelope>(json, Json);
                if (parsed?.Results is null) return;
                foreach (var r in parsed.Results)
                    if (r.Index >= 0 && r.Index < chunk.Count)
                        result[chunk[r.Index].Index] =
                            new UnitVerdict(chunk[r.Index].Index, Normalize(r.Verdict), r.Reason ?? string.Empty);
            }
        }
        catch (JsonException) { /* Chunk unbeantwortet -> oben als 'unclassified' sichtbar */ }
    }

    private static string ProfileFor(string artifactType, string promptVersion)
    {
        var profiles = promptVersion switch
        {
            "v2.2" => ProfilesV22,
            "v2.3" => ProfilesV23,
            _ => Profiles
        };
        return profiles.TryGetValue(artifactType, out var p) ? p : profiles["generic"];
    }

    private static string NormalizePromptVersion(string? version) => version?.Trim().ToLowerInvariant() switch
    {
        "v2" => "v2",
        "v2.1" => "v2.1",
        "v21" => "v2.1",
        "v2.2" => "v2.2",
        "v22" => "v2.2",
        "v2.3" => "v2.3",
        "v23" => "v2.3",
        _ => "v1"
    };

    private static string Normalize(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "grounded" => "grounded",
        "overstated" => "overstated",
        "fabricated" => "fabricated",
        "not_a_claim" => "not_a_claim",
        "na" => "not_a_claim",
        _ => "not_a_claim"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("results")] List<Result>? Results);

    private sealed record Result(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("verdict")] string? Verdict,
        [property: JsonPropertyName("reason")] string? Reason);

    private sealed record EnvelopeV2([property: JsonPropertyName("results")] List<ResultV2>? Results);

    private sealed record ResultV2(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("evidence")] string? Evidence,
        [property: JsonPropertyName("reasoning")] string? Reasoning,
        [property: JsonPropertyName("verdict")] string? Verdict);
}

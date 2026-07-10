using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// B0 — die DETERMINISTISCHEN Ableitungsgüte-Metriken (N1, N2, R1, R2). Reine Funktion über (abgeleitetes Dokument,
/// Quell-Set, verworfene Anker, Inference-Verdikte); kein LLM. Wird beim Ableiten in <c>derivation-report.json</c>
/// (Feld <c>metrics</c>) gestempelt, sodass jeder Run seinen deterministischen Metrik-Vektor trägt und der Aggregator
/// (<see cref="DerivationMetricsAggregator"/>) offline mitteln kann. Definition: <c>docs/B0-Ableitungsguete-Metrik.md</c>.
/// </summary>
/// <remarks>
/// Zwei Achsen, bewusst NICHT zu einer Zahl verrechnet:
/// <list type="bullet">
///   <item><description><b>NUTZEN</b> — N1 Synthese-Tiefe (Ø Anker/Item + Anteil cross-artifact), N2 Nicht-Trivialität
///     (1 − max. lexikalische Ähnlichkeit zum nächsten Quell-Item).</description></item>
///   <item><description><b>RISIKO</b> — R1 Treue-Verletzung (Anteil {Contradicts, Unrelated}), R2 Anker-Defekt-Rate.
///     R3 (Scope-Creep) ist die EINZIGE Judge-Dimension und liegt separat (<see cref="ScopeCreepChecker"/>).</description></item>
/// </list>
/// N2 bleibt bewusst lexikalisch (reproduzierbar, kein Judge): TF-Kosinus über normalisierte, stopword-bereinigte Tokens.
/// </remarks>
public static class DerivationMetrics
{
    /// <summary>Berechnet die deterministischen Metriken für EINEN Ableitungslauf.</summary>
    /// <param name="doc">Das abgeleitete Dokument (anker-gültige Items).</param>
    /// <param name="sources">Die Quell-Artefakte (für N1-Typisierung + N2-Ähnlichkeit).</param>
    /// <param name="invalid">Anker-defekte Items (Zähler für R2).</param>
    /// <param name="verdicts">Inference-Verdikte je Item (für R1).</param>
    /// <param name="totalGenerated">Nenner für R2 = ALLE generierten Items. Modus-abhängig, daher explizit: strukturiert
    /// verwirft defekte Items (doc + invalid disjunkt → doc.Count + invalid.Count); agentisch schreibt sie und markiert
    /// nur (invalid ⊆ doc → doc.Count). Der Aufrufer kennt die Semantik.</param>
    public static DeterministicMetrics ComputeDeterministic(
        ArtifactDocument doc, SourceArtifactSet sources,
        IReadOnlyList<InvalidAnchor> invalid, IReadOnlyList<InferenceVerdict> verdicts, int totalGenerated)
    {
        // id → Artefakttyp (für cross-artifact) und alle Quell-Text-Vektoren (für N2) EINMAL vorbereiten.
        var typeById = new Dictionary<string, string>(StringComparer.Ordinal);
        var sourceVecs = new List<IReadOnlyDictionary<string, int>>();
        foreach (var src in sources.Sources)
            foreach (var it in src.Items)
            {
                typeById[it.ItemId] = src.ArtifactType;
                var v = TermVector(it.Text);
                if (v.Count > 0) sourceVecs.Add(v);
            }

        var verdictById = verdicts.ToDictionary(v => v.ItemId, v => v.Verdict, StringComparer.Ordinal);

        var perItem = new List<ItemMetric>(doc.Items.Count);
        foreach (var it in doc.Items)
        {
            var anchors = it.SourceArtifactItemIds ?? [];
            var types = anchors.Select(a => typeById.TryGetValue(a, out var t) ? t : null)
                               .Where(t => t is not null).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            var verdict = verdictById.TryGetValue(it.ItemId, out var vk) ? vk : InferenceVerdictKind.Unclear;
            perItem.Add(new ItemMetric(
                ItemId: it.ItemId,
                AnchorCount: anchors.Count,
                CrossArtifact: types >= 2,
                NonTriviality: Math.Round(1.0 - MaxSimilarity(it.Text, sourceVecs), 4),
                Verdict: verdict.ToString()));
        }

        var n = doc.Items.Count;
        return new DeterministicMetrics(
            Items: n,
            InvalidAnchors: invalid.Count,
            TotalGenerated: totalGenerated,
            N1_AvgAnchorsPerItem: Round(perItem.Select(p => (double)p.AnchorCount), n),
            N1_CrossArtifactRate: Round(perItem.Select(p => p.CrossArtifact ? 1.0 : 0.0), n),
            N2_AvgNonTriviality: Round(perItem.Select(p => p.NonTriviality), n),
            R1_FidelityViolationRate: Round(perItem.Select(p =>
                p.Verdict is "Contradicts" or "Unrelated" ? 1.0 : 0.0), n),
            R2_InvalidAnchorRate: totalGenerated == 0 ? 0.0 : Math.Round((double)invalid.Count / totalGenerated, 4),
            PerItem: perItem);
    }

    private static double Round(IEnumerable<double> xs, int count)
        => count == 0 ? 0.0 : Math.Round(xs.Sum() / count, 4);

    // 1 − maxähnlichstes Quell-Item. Leerer Item-Text → 0 (trivial); keine Quellen → 1 (nichts, wozu es trivial wäre).
    private static double MaxSimilarity(string text, IReadOnlyList<IReadOnlyDictionary<string, int>> sourceVecs)
    {
        var v = TermVector(text);
        if (v.Count == 0) return 1.0;               // → NonTriviality 0
        if (sourceVecs.Count == 0) return 0.0;      // → NonTriviality 1
        var max = 0.0;
        foreach (var s in sourceVecs)
        {
            var sim = Cosine(v, s);
            if (sim > max) max = sim;
        }
        return max;
    }

    private static double Cosine(IReadOnlyDictionary<string, int> a, IReadOnlyDictionary<string, int> b)
    {
        // Über die kleinere Menge iterieren (Skalarprodukt); Normen aus den TF-Werten.
        var (small, large) = a.Count <= b.Count ? (a, b) : (b, a);
        long dot = 0;
        foreach (var (k, av) in small)
            if (large.TryGetValue(k, out var bv)) dot += (long)av * bv;
        if (dot == 0) return 0.0;
        var na = Math.Sqrt(a.Values.Sum(x => (double)x * x));
        var nb = Math.Sqrt(b.Values.Sum(x => (double)x * x));
        return na == 0 || nb == 0 ? 0.0 : dot / (na * nb);
    }

    // Normalisierung: Kleinschreibung, Trennung an Nicht-Buchstaben/-Ziffern, Tokens < 3 Zeichen + Stopwörter raus.
    private static IReadOnlyDictionary<string, int> TermVector(string? text)
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(text)) return map;
        var tok = new System.Text.StringBuilder();
        void Flush()
        {
            if (tok.Length >= 3)
            {
                var w = tok.ToString();
                if (!Stopwords.Contains(w)) map[w] = map.GetValueOrDefault(w) + 1;
            }
            tok.Clear();
        }
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch)) tok.Append(char.ToLowerInvariant(ch));
            else Flush();
        }
        Flush();
        return map;
    }

    // Kleine DE+EN-Stoppwortliste (Funktionswörter, die Ähnlichkeit künstlich aufblähen). Bewusst knapp gehalten.
    private static readonly HashSet<string> Stopwords = new(StringComparer.Ordinal)
    {
        "der","die","das","und","oder","aber","nicht","ein","eine","einer","eines","einem","einen","den","dem","des",
        "für","mit","von","aus","auf","bei","zur","zum","als","auch","sich","ist","sind","war","wird","werden","kann",
        "muss","soll","sollen","dass","wenn","dann","durch","über","unter","nach","vor","wie","bzw","etc","sowie",
        "the","and","for","with","that","this","are","was","will","would","can","must","should","from","into","not",
        "have","has","been","their","its","which","when","then","than","such","also","per","via"
    };
}

/// <summary>Deterministische Per-Item-Metrik (B0).</summary>
public sealed record ItemMetric(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("anchorCount")] int AnchorCount,
    [property: JsonPropertyName("crossArtifact")] bool CrossArtifact,
    [property: JsonPropertyName("nonTriviality")] double NonTriviality,
    [property: JsonPropertyName("verdict")] string Verdict);

/// <summary>Deterministischer Metrik-Vektor EINES Ableitungslaufs (B0). N* = Nutzen, R* = Risiko; Item-Anzahl = deskriptiv.</summary>
public sealed record DeterministicMetrics(
    [property: JsonPropertyName("items")] int Items,
    [property: JsonPropertyName("invalidAnchors")] int InvalidAnchors,
    [property: JsonPropertyName("totalGenerated")] int TotalGenerated,
    [property: JsonPropertyName("n1_avgAnchorsPerItem")] double N1_AvgAnchorsPerItem,
    [property: JsonPropertyName("n1_crossArtifactRate")] double N1_CrossArtifactRate,
    [property: JsonPropertyName("n2_avgNonTriviality")] double N2_AvgNonTriviality,
    [property: JsonPropertyName("r1_fidelityViolationRate")] double R1_FidelityViolationRate,
    [property: JsonPropertyName("r2_invalidAnchorRate")] double R2_InvalidAnchorRate,
    [property: JsonPropertyName("perItem")] IReadOnlyList<ItemMetric> PerItem);

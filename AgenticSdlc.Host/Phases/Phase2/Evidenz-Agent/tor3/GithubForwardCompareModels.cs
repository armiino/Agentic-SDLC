using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// T3.6 — Drift-Fixture: ein Issue-Snapshot + unmapped-PBI-Faelle mit GOLD-Label (was der ideale Matcher tun sollte).
// Bewusst die Faelle aus plan-tor3 §9: Paraphrase, entfernte Requirement-IDs (Dedup-Recall) sowie klarer Treffer /
// echte Neuanlage (wo Determinismus reicht).
public sealed record DriftFixtureDocument(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("issues")] IReadOnlyList<GithubIssueSnapshot> Issues,
    [property: JsonPropertyName("cases")] IReadOnlyList<DriftCase> Cases);

public sealed record DriftCase(
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("requirementTexts")] IReadOnlyList<string> RequirementTexts,
    [property: JsonPropertyName("gold")] DriftGold Gold,
    [property: JsonPropertyName("note")] string? Note);

public sealed record DriftGold(
    [property: JsonPropertyName("kind")] string Kind,          // LINK | CREATE
    [property: JsonPropertyName("issueNumber")] int? IssueNumber);

public sealed record CompareReport(
    [property: JsonPropertyName("fixture")] string Fixture,
    [property: JsonPropertyName("candidate")] string Candidate,   // "deterministic" | "agent"
    [property: JsonPropertyName("threshold")] int? Threshold,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("correct")] int Correct,
    [property: JsonPropertyName("accuracy")] double Accuracy,
    [property: JsonPropertyName("dedupRecall")] double DedupRecall,
    [property: JsonPropertyName("linkPrecision")] double LinkPrecision,
    [property: JsonPropertyName("goldLinks")] int GoldLinks,
    [property: JsonPropertyName("goldCreates")] int GoldCreates,
    [property: JsonPropertyName("cases")] IReadOnlyList<CaseResult> Cases);

public sealed record CaseResult(
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("goldKind")] string GoldKind,
    [property: JsonPropertyName("goldIssue")] int? GoldIssue,
    [property: JsonPropertyName("decisionKind")] string DecisionKind,
    [property: JsonPropertyName("decisionIssue")] int? DecisionIssue,
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("note")] string? Note);

// Reine Metrik-Engine: vergleicht Kandidaten-Entscheidungen (pbiId -> LINK #n / CREATE) gegen die Gold-Labels.
public static class GithubForwardCompare
{
    public const string CorrectLink = "correct-link";
    public const string CorrectCreate = "correct-create";
    public const string MissedDedup = "missed-dedup";   // gold LINK, Kandidat CREATE -> Duplikat-Risiko (der Kernfehler)
    public const string WrongLink = "wrong-link";       // gold LINK #n, Kandidat LINK #m
    public const string FalseLink = "false-link";       // gold CREATE, Kandidat LINK -> faelschlich zugeordnet

    public static CompareReport Evaluate(
        DriftFixtureDocument fixture,
        string candidate,
        int? threshold,
        IReadOnlyDictionary<string, (string Kind, int? Issue)> decisions)
    {
        var results = new List<CaseResult>();
        foreach (var c in fixture.Cases)
        {
            var (kind, issue) = decisions.TryGetValue(c.PbiId, out var d) ? d : ("(none)", (int?)null);
            var verdict = Verdict(c.Gold, kind, issue);
            results.Add(new CaseResult(c.PbiId, c.Gold.Kind, c.Gold.IssueNumber, kind, issue, verdict, c.Note));
        }

        var total = results.Count;
        var correct = results.Count(r => r.Verdict is CorrectLink or CorrectCreate);
        var goldLinks = fixture.Cases.Count(c => IsLink(c.Gold.Kind));
        var goldCreates = total - goldLinks;
        var correctLinks = results.Count(r => r.Verdict == CorrectLink);
        var candidateLinks = results.Count(r => IsLink(r.DecisionKind));

        return new CompareReport(
            fixture.Name, candidate, threshold, total, correct,
            Accuracy: total == 0 ? 1 : Math.Round((double)correct / total, 3),
            DedupRecall: goldLinks == 0 ? 1 : Math.Round((double)correctLinks / goldLinks, 3),
            LinkPrecision: candidateLinks == 0 ? 1 : Math.Round((double)correctLinks / candidateLinks, 3),
            GoldLinks: goldLinks, GoldCreates: goldCreates, Cases: results);
    }

    private static string Verdict(DriftGold gold, string kind, int? issue)
    {
        if (IsLink(gold.Kind))
            return !IsLink(kind) ? MissedDedup : (issue == gold.IssueNumber ? CorrectLink : WrongLink);
        return IsLink(kind) ? FalseLink : CorrectCreate;
    }

    private static bool IsLink(string kind) => string.Equals(kind, GithubForwardKind.Link, StringComparison.OrdinalIgnoreCase);
}

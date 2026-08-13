using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// In-Sync-Erkennung am Forward-Seed (Autor-Fund 13.08. nach dem Voll-Stempel-Lauf R6): ein gemapptes,
// gestempeltes Issue, dessen Inhalt EXAKT der aktuellen Core-Projektion entspricht, bekommt NoChange statt
// UPDATE — der Plan wird damit der ehrliche Drift-Report („was hängt WIRKLICH hinterher?") und idempotente
// Re-Writes entfallen. Wächter: Unknown-Stempel heilt weiter per UPDATE, Mensch-Edits sperren weiter (FLAG_DRIFT).
public sealed class GithubForwardSeedNoChangeTests
{
    private const string Quelle = "Forward-Update aus Core-PBI (deterministisch)";   // exakt der Seed-Render

    private static GithubSyncEntry Entry(string ak = "Login klappt") => new(
        PbiId: "PBI-7", Title: "Profil-Detailansicht", Status: "active", Readiness: "backlog_ready",
        CoveredRequirementIds: ["REQ-1"], BlockedByOpenDecision: false, GithubIssue: "gh#7",
        AcceptanceCriteria: [ak], Statement: "Als Pflegekraft will ich das Profil sehen.");

    private static (GithubMappingRecord Mapping, GithubIssueSnapshot Issue) StampedInSync(GithubSyncEntry e)
    {
        var body = GithubIssueTemplate.Render(e, Quelle);
        var mapping = new GithubMappingRecord(e.PbiId, 7, null, "o/r", "open",
            ProjectedTitleHash: GithubProjectionHash.Compute(e.Title),
            ProjectedBodyHash: GithubProjectionHash.Compute(body));
        var issue = new GithubIssueSnapshot(7, null, e.Title, body, "open", [], null, null);
        return (mapping, issue);
    }

    private static GithubForwardOp Single(GithubSyncEntry e, GithubMappingRecord m, GithubIssueSnapshot i)
        => Assert.Single(GithubForwardSeed.Seed([e], new Dictionary<string, GithubMappingRecord> { [e.PbiId] = m }, [i]).DeterministicOps);

    [Fact]
    public void Gestempelt_und_inhaltsgleich_ist_NoChange_kein_Update()
    {
        var e = Entry();
        var (mapping, issue) = StampedInSync(e);
        var op = Single(e, mapping, issue);
        Assert.Equal(GithubForwardKind.NoChange, op.Kind);                 // DER Fix: kein ewiges UPDATE mehr
        Assert.Equal(7, op.TargetIssueNumber);
        Assert.Contains("In Sync", op.Rationale);
    }

    [Fact]
    public void Core_weiterentwickelt_bleibt_UPDATE()
    {
        var e = Entry();
        var (mapping, issue) = StampedInSync(e);                          // Stempel+Issue = ALTER Stand
        var evolved = e with { AcceptanceCriteria = ["Login klappt", "2FA Pflicht"] };   // Core ist weiter
        var op = Single(evolved, mapping, issue);
        Assert.Equal(GithubForwardKind.UpdateIssue, op.Kind);             // echter Drift → schreiben
        Assert.Contains("2FA Pflicht", op.Body);
    }

    [Fact]
    public void Alt_Issue_ohne_Stempel_bleibt_UPDATE_damit_der_Write_stempelt()
    {
        var e = Entry();
        var body = GithubIssueTemplate.Render(e, Quelle);
        var mapping = new GithubMappingRecord(e.PbiId, 7, null, "o/r", "open");          // KEIN Stempel (Unknown)
        var issue = new GithubIssueSnapshot(7, null, e.Title, body, "open", [], null, null);   // inhaltsgleich!
        var op = Single(e, mapping, issue);
        Assert.Equal(GithubForwardKind.UpdateIssue, op.Kind);             // Heil-Semantik hat Vorrang vor NoChange
        Assert.Contains("dieser Write stempelt", op.Rationale);
    }

    [Fact]
    public void Mensch_editiertes_Issue_bleibt_FLAG_DRIFT()
    {
        var e = Entry();
        var (mapping, _) = StampedInSync(e);
        var edited = new GithubIssueSnapshot(7, null, e.Title, "Hand-Edit vom Team!", "open", [], null, null);
        var op = Single(e, mapping, edited);
        Assert.Equal(GithubForwardKind.FlagDrift, op.Kind);               // Sperre unangetastet (Regression)
        Assert.Contains("DRIFT-SPERRE", op.Rationale);
    }
}

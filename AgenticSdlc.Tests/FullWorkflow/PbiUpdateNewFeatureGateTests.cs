using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// O4a (deterministisch, kein LLM): das Placement kann jetzt NEW_FEATURE ausgeben (Fall C — kein bestehendes
// Feature passt). Der Gate akzeptiert NEW_FEATURE als gültige Placement-Coverage, verlangt aber ein Label (der
// Seeder-Adapter in O4b braucht es zum Anlegen). KEIN UNKNOWN_FEATURE, weil das Feature erst entsteht.
public sealed class PbiUpdateNewFeatureGateTests
{
    private static ProjectStateItem Feature(string id, string label) => new ProjectStateItem(
        id, "feature", label, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static PbiStateChangeOperation NewFeature(string req, string? label)
        => new("NEW_FEATURE", req, null, null, null, null, "eigenständiges neues Thema", ProposedFeatureLabel: label);

    private static PbiStateChangePlanDocument Plan(params PbiStateChangeOperation[] ops)
        => new(1, "p1", DateTime.UnixEpoch, "src", ops);

    [Fact]
    public void NewFeature_mit_Label_passt_und_deckt_das_Requirement()
    {
        var report = PbiUpdateGate.Check(
            Core(Feature("FC-01", "Bestehend")),
            Plan(NewFeature("REQ-9", "Schichtübergabe")),
            new HashSet<string> { "REQ-9" });

        Assert.True(report.Pass);
        Assert.DoesNotContain(report.Errors, e => e.Code == "FEATURE_LABEL_REQUIRED");
    }

    [Fact]
    public void NewFeature_ohne_Label_faellt()
    {
        var report = PbiUpdateGate.Check(
            Core(Feature("FC-01", "Bestehend")),
            Plan(NewFeature("REQ-9", null)),
            new HashSet<string> { "REQ-9" });

        Assert.False(report.Pass);
        Assert.Contains(report.Errors, e => e.Code == "FEATURE_LABEL_REQUIRED");
    }

    [Fact]
    public void NewFeature_wirft_kein_UNKNOWN_FEATURE_oder_FEATURE_REQUIRED()
    {
        // Anders als NEW_PBI: NEW_FEATURE referenziert kein bestehendes Feature -> darf nicht UNKNOWN_FEATURE
        // oder FEATURE_REQUIRED werfen (das Feature entsteht erst im Seeder-Adapter, O4b).
        var report = PbiUpdateGate.Check(
            Core(Feature("FC-01", "Bestehend")),
            Plan(NewFeature("REQ-9", "Neues Thema")),
            new HashSet<string> { "REQ-9" });

        Assert.DoesNotContain(report.Errors, e => e.Code is "UNKNOWN_FEATURE" or "FEATURE_REQUIRED");
    }
}

using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Core;
using Xunit;

// A′ Schritt 1 (steward/graph-entry-vs-werkbank.md §9/§11) — der GETEILTE clarify-Kern, isoliert (kein Graph, kein LLM):
// die LLM-Alignment-Naht wird als Fake injiziert. Bewiesen: (1) derselbe fachliche Plan wie der Standalone-Builder
// (Werkbank-Parität) + Alignment angehängt · (2) der PFLICHT-Checker liefert einen ECHTEN Verdict (kein hardcodiertes
// Pass; UNKNOWN_TARGET wird laut) · (3) leerer Plan ⇒ KEIN LLM-Schritt.
namespace AgenticSdlc.Tests.FullWorkflow;

public sealed class ClarifyEntryPlanTests
{
    private static ProjectStateItem Pbi(string id, string title) => new ProjectStateItem(
        id, "pbi", title, "test", null, 1, "run", null, null, null, null, [], [],
        new Dictionary<string, string>(),
        Pbi: new PbiPayload(null, title, [], [], [], null, null, null, null))
        .WithStatus(CoreStatus.From("needs_clarify"));

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static PbiStateChangePlanDocument MarkChangedPlan(string pbiId)
        => new(1, "p1", DateTime.UnixEpoch, "run-1",
            [new PbiStateChangeOperation("MARK_CHANGED", "", pbiId, null, null, null, "test",
                AuthorAnswerRef: "chat:steward#1", AuthorAnswerText: "Antwort")], null);

    [Fact]
    public async Task AssembleAsync_erzeugt_den_Builder_Plan_und_haengt_Alignments_an()
    {
        var core = Core(Pbi("PBI-01", "Rechtemodell"));
        var answers = new[] { new ClarifySweepAnswer("PBI-01", "Drei Rollen: Personal, Angehoerige, Leitung.") };
        var fake = new PbiAlignment("PBI-01", "Titel", "Statement", ["AK1"], "r", ["chat:steward#1"]);
        AnswerAlignSeam align = (t, ct) => Task.FromResult<IReadOnlyList<PbiAlignment>>([fake]);

        var (plan, targets, skipped) = await ClarifyEntryPlan.AssembleAsync(core, answers, "run-1", align);

        // Werkbank-Paritaet: identische Ops/Targets wie der Standalone-Builder direkt.
        var (builderPlan, builderTargets, _) = ClarifySweepPlanBuilder.Build(core, answers, "run-1");
        Assert.Equal(builderPlan.Operations.Count, plan.Operations.Count);
        Assert.Equal(builderTargets.Count, targets.Count);
        Assert.Equal("MARK_CHANGED", Assert.Single(plan.Operations).Kind);
        Assert.Equal("PBI-01", plan.Operations[0].PbiId);
        Assert.Empty(skipped);
        // Alignment ueber die injizierte Naht angehaengt.
        Assert.NotNull(plan.Alignments);
        Assert.Single(plan.Alignments!);
    }

    [Fact]
    public void Validate_liefert_echten_Verdict_aus_dem_Checker_kein_hardcodiertes_Pass()
    {
        var core = Core(Pbi("PBI-01", "Rechtemodell"));
        var plan = MarkChangedPlan("PBI-01");

        var v = ClarifyEntryPlan.Validate(core, plan);

        Assert.True(v.Pass);
        Assert.Equal(GateDecision.Pass, v.Decision);
        // Beweis „nicht hardcodiert": identisch zur direkten Checker-Rechnung.
        var expected = PbiUpdateGate.Decide(
            PbiUpdateGate.Check(core, plan, new HashSet<string>(StringComparer.Ordinal)), 1, 1);
        Assert.Equal(expected, v.Decision);
    }

    [Fact]
    public void Validate_zeigt_UNKNOWN_TARGET_laut_und_zieht_nicht_glatt()
    {
        var core = Core(Pbi("PBI-01", "Rechtemodell"));
        var plan = MarkChangedPlan("PBI-XX");   // Ziel existiert NICHT im Core

        var v = ClarifyEntryPlan.Validate(core, plan);

        Assert.False(v.Pass);
        Assert.Contains(v.Report.Errors, e => e.Code == "UNKNOWN_TARGET");
        Assert.NotEqual(GateDecision.Pass, v.Decision);
    }

    [Fact]
    public void AsVerdict_baut_den_Verdict_NUR_aus_der_Validation_und_zieht_nichts_glatt()
    {
        var core = Core(Pbi("PBI-01", "Rechtemodell"));
        var ctx = new PbiUpdateWfContext(core, [], "run-1", "/out", DryRun: false, MaxAttempts: 1);

        // Pass: Verdict trägt Report/Decision/Plan WÖRTLICH aus der Validation; ctx durchgereicht; Applied leer (clarify).
        var okValidation = ClarifyEntryPlan.Validate(core, MarkChangedPlan("PBI-01"));
        var okVerdict = ClarifyEntryPlan.AsVerdict(okValidation, ctx);
        Assert.Same(okValidation.Report, okVerdict.Report);
        Assert.Equal(okValidation.Decision, okVerdict.Decision);
        Assert.Same(okValidation.Plan, okVerdict.Plan);
        Assert.Same(ctx, okVerdict.Ctx);
        Assert.Empty(okVerdict.Ctx.Applied);
        Assert.Empty(okVerdict.Unplaced);

        // Non-Pass: der Verdict spiegelt den Fehl-Befund (kein glattgezogenes Pass am Verdict-Bau).
        var badValidation = ClarifyEntryPlan.Validate(core, MarkChangedPlan("PBI-XX"));
        var badVerdict = ClarifyEntryPlan.AsVerdict(badValidation, ctx);
        Assert.False(badVerdict.Report.Pass);
        Assert.NotEqual(GateDecision.Pass, badVerdict.Decision);
    }

    [Fact]
    public async Task AssembleAsync_ohne_gueltige_Antwort_ruft_die_LLM_Naht_NICHT()
    {
        var core = Core(Pbi("PBI-01", "Rechtemodell"));
        var answers = new[] { new ClarifySweepAnswer("PBI-UNBEKANNT", "egal") };  // kein PBI im Core → skip
        var called = false;
        AnswerAlignSeam spy = (t, ct) => { called = true; return Task.FromResult<IReadOnlyList<PbiAlignment>>([]); };

        var (plan, _, skipped) = await ClarifyEntryPlan.AssembleAsync(core, answers, "run-1", spy);

        Assert.Empty(plan.Operations);
        Assert.NotEmpty(skipped);
        Assert.False(called);   // leerer Plan ⇒ kein LLM-Schritt
    }
}

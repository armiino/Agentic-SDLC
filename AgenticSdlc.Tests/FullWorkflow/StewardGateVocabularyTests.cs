using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Steward;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Selbstbeschreibende Chat-Gates (18.08., Politur): die Gate-Vorlagen liefern ihre Optionen SELBST —
// Labels wörtlich aus den Review-Adaptern (E0-Quelle der UI), Codes je Chat-Vertrag. Vorher stand das
// Vokabular als abgeschriebene Tabelle im Steward-Prompt (Drift-Quelle). Diese Pins sichern:
// (1) Label-Herkunft = Adapter (ändert die UI ein Label, wandert es automatisch mit),
// (2) die Bahn-Codes entsprechen GENAU dem, was die Submit-Tools validieren.
public sealed class StewardGateVocabularyTests
{
    [Fact]
    public void Ingest_Optionen_tragen_Adapter_Labels_mit_Chat_Code_reject()
    {
        var refine = StewardGateVocabulary.ForIngest(StateChangeKind.Refine);
        Assert.Equal(["apply", "reject"], refine.Select(o => o.Code));      // Chat spricht reject, nie skip
        Assert.Equal("✓ Verfeinerung übernehmen", refine[0].Label);         // Label wörtlich aus dem Adapter
        Assert.Equal("Nicht übernehmen (Begründung Pflicht)", refine[1].Label);

        // Op-Art-spezifische Labels kommen mit (der Adapter unterscheidet — die Chat-Vorlage erbt das):
        Assert.Equal("✓ Neu anlegen", StewardGateVocabulary.ForIngest(StateChangeKind.New)[0].Label);
        Assert.Equal("✓ Als offene Frage aufnehmen", StewardGateVocabulary.ForIngest(StateChangeKind.OpenQuestion)[0].Label);
    }

    [Fact]
    public void Decision_Optionen_mappen_auf_die_Submit_Codes_und_erben_die_Adapter_Labels()
    {
        var opts = StewardGateVocabulary.ForDecision();
        Assert.Equal(["resolve/KEEP_ORIGINAL", "resolve/ADOPT_NEW", "resolve/REFINE", "defer"],
            opts.Select(o => o.Code));
        // Labels = EXAKT die Adapter-Liste (eine Quelle, keine Kopie):
        Assert.Equal(PipelineDecisionReviewAdapter.ResolutionOptions.Select(o => o.Label),
            opts.Select(o => o.Label));
    }

    [Fact]
    public void Forward_und_PbiUpdate_Optionen_kommen_eins_zu_eins_aus_den_Adaptern()
    {
        Assert.Equal(GithubForwardReviewAdapter.OpOptions.Select(o => (o.Value, o.Label)),
            StewardGateVocabulary.ForForward().Select(o => (o.Code, o.Label)));
        // R-14 D2: der dritte Weg (to_decision) ist auch im Chat vorlegbar.
        Assert.Contains(StewardGateVocabulary.ForPbiOps(), o => o.Code == "to_decision");
        Assert.Equal(PbiUpdateReviewAdapter.AlignmentOptions.Select(o => (o.Value, o.Label)),
            StewardGateVocabulary.ForPbiAlignments().Select(o => (o.Code, o.Label)));
    }
}

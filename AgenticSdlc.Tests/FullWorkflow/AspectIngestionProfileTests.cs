using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A1a (05.08.): die Aspekt-Naht der Ingestion. Golden-Test = das Requirement-Profil reproduziert das
// heutige Verhalten EXAKT (bit-gleich); Zweitprofil-Test = Filter/Prefix/ItemType/Tool-Name kommen wirklich
// aus dem Profil (Wegwerf-Profil "widget" — NICHTS davon ist im Graphen verdrahtet, das ist A1b/A1d);
// Abgrenzungs-Test = der 9g-Fragen-Pfad ist NICHT mitparametrisiert (Teil 5/D-2: Querschnitt).
public sealed class AspectIngestionProfileTests
{
    private static readonly AspectIngestionProfile Widget = new(
        Aspect: "widget", IdPrefix: "WID", ItemLabel: "Widget",
        AgentName: "WidgetAgent", PromptName: "WidgetAgent1",
        ListCoreToolName: "list_core_widgets", ResolverTaskText: "widget-task", ExecutorIdPrefix: "WidgetIngestion", EventPrefix: "WIDGET_INGEST",
        CarriesQuestionLane: true);

    private static ProjectStateItem Item(string id, string type, string text = "t") => new ProjectStateItem(
        id, type, text, "MEETING", null, 1,
        "r", null, null, null, null, ["SL-1"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, "plan-test", DateTime.UnixEpoch, "src", [.. ops]);

    private static StateChangeOperation Op(string incoming, string kind, string? target = null)
        => new(incoming, kind, $"Statement {incoming}", target, null, ["SL-1"], "rationale");

    // ---- Golden: das Requirement-Profil IST das heutige Verhalten ----

    [Fact]
    public void Requirement_Profil_reproduziert_Namen_Prefix_Filter_exakt()
    {
        var p = AspectIngestionProfile.Requirement;
        Assert.Equal("requirement", p.Aspect);
        Assert.Equal("REQ", p.IdPrefix);
        Assert.Equal("RequirementIngestionAgent", p.AgentName);
        Assert.Equal("RequirementIngestionAgent1", p.PromptName);
        Assert.Equal("list_core_requirements", p.ListCoreToolName);
        Assert.Equal(IngestionResolveTask.Text, p.ResolverTaskText);   // Task-Text = dieselbe Quelle, keine Kopie
        Assert.True(p.Matches(Item("X", "Requirement")));              // case-insensitive wie bisher
        Assert.False(p.Matches(Item("X", "architecture")));
    }

    [Fact]
    public void Requirement_Profil_erzeugt_die_bisherigen_Tool_Namen_und_Beschreibungen()
    {
        var run = new RunContext(RunId.New(), "test-aspect-profile"); run.EnsureFolders();
        var tools = new IngestionTools(Doc(), Doc(), new ShowAllRequirementRetriever(), run, AspectIngestionProfile.Requirement).Build();

        var byName = tools.ToDictionary(t => t.Name, StringComparer.Ordinal);
        Assert.Contains("list_core_requirements", byName.Keys);        // Tool-Name = Vertrag mit dem Prompt
        Assert.Contains("get_incoming_items", byName.Keys);
        // Golden-Strings: die Templates ergeben für req BYTE-GLEICHE Beschreibungen (Bit-gleich-Beleg).
        Assert.Equal("Listet die eingehenden Requirement-Items dieses Meetings (incomingItemId, text, sourceClaimIds), die aufgeloest werden muessen.",
            byName["get_incoming_items"].Description);
        Assert.Equal("Listet die bestehenden Requirement-Entitaeten des Core als Kandidaten (entityId, identityKey, status, text).",
            byName["list_core_requirements"].Description);
        Assert.Equal("Sucht in den Core-Requirements nach Stichworten (entityId/text).", byName["search_core"].Description);
    }

    // ---- Zweitprofil: die Naht trägt wirklich (nichts req-hartes übrig) ----

    [Fact]
    public void Zweitprofil_steuert_Gate_Coverage_ueber_den_Aspekt_Filter()
    {
        var delta = Doc(Item("W-1", "widget"), Item("R-1", "requirement"));
        var core = Doc();

        // Widget-Profil: NUR das widget-Item ist Coverage-Bürger — die fehlende Op für R-1 ist KEIN Fehler.
        var report = IngestionGate.Check(delta, core, Plan(Op("W-1", StateChangeKind.New)), Widget);
        Assert.True(report.Pass, string.Join("; ", report.Errors.Select(e => e.Code + ":" + e.Message)));

        // Gegenprobe: ohne Op für W-1 schlägt die Coverage an — mit der widget-Id.
        var missing = IngestionGate.Check(delta, core, Plan(), Widget);
        Assert.Contains(missing.Errors, e => e.IncomingItemId == "W-1");
        Assert.DoesNotContain(missing.Errors, e => e.IncomingItemId == "R-1");
    }

    [Fact]
    public void Zweitprofil_steuert_Prefix_und_ItemType_im_Apply()
    {
        var delta = Doc(Item("W-1", "widget", "Ein neues Widget"));
        var core = Doc();

        var (updated, report, _) = IngestionApply.Apply(core, delta, Plan(Op("W-1", StateChangeKind.New)),
            new HashSet<string>(StringComparer.Ordinal) { "W-1" }, "run-1", Widget);

        Assert.Single(report.Applied);
        var created = Assert.Single(updated.Items);
        Assert.StartsWith("WID-", created.ItemId);                     // Prefix aus dem Profil
        Assert.Equal("widget", created.ItemType);                      // NEW-Fall schreibt den Profil-Aspekt (D-1-Fund!)
    }

    [Fact]
    public void Fragen_Spur_folgt_dem_Profil_Flag()
    {
        // 9i (löst D-2 bewusst ab): die 9g-Fragen-Spur gehört GENAU EINEM Strip (CarriesQuestionLane) —
        // sonst müsste bei gemischten Deltas JEDER Strip die Fragen covern (Doppel-Coverage/Doppel-DEC).
        // Spur-tragendes Profil: Fragen sind Coverage-Bürger (9g-Verhalten unverändert).
        var delta = Doc(Item("OQ-1", "open_question", "Ist X geklärt?"));
        var core = Doc();

        var missing = IngestionGate.Check(delta, core, Plan(), Widget);
        Assert.Contains(missing.Errors, e => e.IncomingItemId == "OQ-1");   // Frage verlangt Op (Flag=true)

        var ok = IngestionGate.Check(delta, core, Plan(Op("OQ-1", StateChangeKind.OpenQuestion)), Widget);
        Assert.True(ok.Pass, string.Join("; ", ok.Errors.Select(e => e.Code)));

        // Spur-loses Profil (arch, Flag=false): weder Coverage-Pflicht noch erlaubtes Op-Ziel —
        // gepinnt in GithubQuestionOriginTests.Fragen_Spur_gehoert_nur_dem_Requirement_Strip.
        Assert.False(AspectIngestionProfile.Architecture.CarriesQuestionLane);
        Assert.True(AspectIngestionProfile.Requirement.CarriesQuestionLane);
    }
}

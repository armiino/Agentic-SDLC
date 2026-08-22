using AgenticSdlc.Host.FullWorkflow.Analyst;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C4-Kreislauf (22.08., ⚖ „harter Kreislauf statt Wissens-Kopplung"): Aspekt-Färbung an DECs (additiv,
// neutral für alle anderen) · Antwort-Anker decisionRef→answersDecision · dritte Auflöse-Option
// NO_TRUTH_NEEDED (nur aspect-markiert, ziellos) · §3 = deterministische Projektion des DEC-Topfs ·
// Frische-Wächter beidseitig · Wahrheits-Kanten im Drafting-Input.
public sealed class C4LoopTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted",
        Dictionary<string, string>? meta = null) => new ProjectStateItem(
        id, type, text, "MEETING", null, 1,
        "delta-run", null, null, null, null, ["claim-1"], [],
        meta ?? new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items) => new("p", 4, T, [], [.. items], [], [], []);

    private static ProjectStateItem ArchDec(string id, string text, bool open = true, Dictionary<string, string>? extraMeta = null)
    {
        var meta = extraMeta ?? new Dictionary<string, string>();
        meta[DecisionAspectMeta.Key] = DecisionAspectMeta.Architecture;
        return Item(id, "decision", text, open ? "open_decision" : "resolved", meta);
    }

    // ── ① Färbung: Diktat + Analyst + Mint-Carry ──────────────────────────────────────────────

    [Fact]
    public void Diktat_traegt_aspect_und_decisionRef_validiert_und_als_Meta()
    {
        var (_, errors) = AuthorFrontDeltaBuilder.Build(
            [new AuthorStatement("x", "question", Aspect: "banane"),
             new AuthorStatement("y", "architecture", DecisionRef: "REQ-9")], "s");
        Assert.Equal(2, errors.Count);                                        // beide Anker LAUT validiert

        var (delta, ok) = AuthorFrontDeltaBuilder.Build(
            [new AuthorStatement("Wie wird X umgesetzt?", "question", Aspect: "architecture"),
             new AuthorStatement("X wird via Y umgesetzt.", "architecture", DecisionRef: "DEC-011")], "s");
        Assert.Empty(ok);
        Assert.Equal(DecisionAspectMeta.Architecture, delta!.Items[0].Metadata[DecisionAspectMeta.Key]);
        Assert.Equal("DEC-011", delta.Items[1].Metadata[DecisionAnswerMeta.Key]);
    }

    [Fact]
    public void Analyst_arch_Frage_ist_gefaerbt_nfr_Frage_nicht()
    {
        var delta = AnalystDeltaBuilder.Build(
            [new AnalystFinding("arch", "question", "Frage A?", "h", "functional", ["REQ-1"]),
             new AnalystFinding("nfr", "question", "Frage B?", "h", "nfr:privacy", ["REQ-1"])], "run-1");
        Assert.Equal(DecisionAspectMeta.Architecture, delta.Items[0].Metadata[DecisionAspectMeta.Key]);
        Assert.False(delta.Items[1].Metadata.ContainsKey(DecisionAspectMeta.Key));
    }

    [Fact]
    public void Mint_traegt_die_Faerbung_an_die_DEC_und_Ingest_den_Antwort_Anker_in_die_Wahrheit()
    {
        var incoming = Item("AF-1", "open_question", "Wie wird X umgesetzt?", "baseline",
            new Dictionary<string, string> { [DecisionAspectMeta.Key] = DecisionAspectMeta.Architecture });
        var dec = MeetingQuestionMint.NewDecision("DEC-001", incoming.Text, incoming, [], "run");
        Assert.True(DecisionAspectMeta.IsArchitecture(dec));                  // Carry ins DEC

        // Antwort-Anker: NEW-Op trägt answersDecision in das entstehende Wahrheits-Item (geteilte IngestMeta-Naht).
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));
        var answer = Item("AF-2", "requirement", "X wird via Y umgesetzt.", "baseline",
            new Dictionary<string, string> { [DecisionAnswerMeta.Key] = "DEC-001" });
        var delta = Doc(answer);
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, "p1", T, "d",
            [new StateChangeOperation("AF-2", StateChangeKind.New, answer.Text, null, null, ["claim-1"], "neu")]);
        var (updated, report, _) = IngestionApply.Apply(core, delta, plan,
            new HashSet<string>(StringComparer.Ordinal) { "AF-2" }, "ingest-run");
        var truth = updated.Items.Single(i => i.ItemId == report.Applied.Single().EntityId);
        Assert.Equal("DEC-001", DecisionAnswerMeta.Of(truth));
        Assert.True(DecisionAnswerMeta.HasAnsweringTruth(updated, "DEC-001"));
    }

    [Fact]
    public void Gate_Antwort_Diktat_darf_nie_in_die_Frage_gefaltet_werden()   // R-74
    {
        var core = Doc(Item("REQ-1", "requirement", "bestehend"), ArchDec("DEC-009", "Wie Grenze durchsetzen?"));
        var answer = Item("AF-1", "architecture", "Grenze via Security Rules.", "baseline",
            new Dictionary<string, string> { [DecisionAnswerMeta.Key] = "DEC-009" });
        var delta = Doc(answer);

        StateChangePlanDocument Plan(string kind, string? target = null) => new(
            StateChangePlanDocument.CurrentSchemaVersion, "p1", T, "d",
            [new StateChangeOperation("AF-1", kind, answer.Text, target, null, ["claim-1"], "r")]);

        var bad = IngestionGate.Check(delta, core, Plan(StateChangeKind.AlreadyDecided, "DEC-009"),
            AspectIngestionProfile.Architecture);
        var issue = Assert.Single(bad.Errors, e => e.Code == "ANSWER_NEEDS_TRUTH");
        Assert.Equal(Repairability.Repairable, issue.Repairability);          // Repair-Loop kann fixen

        Assert.True(IngestionGate.Check(delta, core, Plan(StateChangeKind.New),
            AspectIngestionProfile.Architecture).Pass);                       // die Antwort wird Wahrheit

        // R-74b: auch der REFINE-Weg trägt den Anker (verfeinert die Antwort bestehende Wahrheit,
        // ist das Ziel-Item das answering-Item).
        var coreMitZiel = Doc(Item("REQ-1", "requirement", "bestehend"),
            Item("ARCH-11", "architecture", "Alter Rahmen."), ArchDec("DEC-009", "Wie Grenze durchsetzen?"));
        var (refined, _, _) = IngestionApply.Apply(coreMitZiel, delta,
            Plan(StateChangeKind.Refine, "ARCH-11"), new HashSet<string>(StringComparer.Ordinal) { "AF-1" }, "run-r",
            AspectIngestionProfile.Architecture);
        Assert.Equal("DEC-009", DecisionAnswerMeta.Of(refined.Items.Single(i => i.ItemId == "ARCH-11")));
    }

    // ── ③ dritte Auflöse-Option (streng geguardet) ────────────────────────────────────────────

    [Fact]
    public void NoTruthNeeded_nur_ziellos_und_nur_aspect_markiert()
    {
        var core = Doc(ArchDec("DEC-010", "Wie X?"), Item("DEC-020", "decision", "Frage ohne Färbung?", "open_decision"));

        var (_, p1) = DecisionResolutionDerivation.Derive(core,
            DecisionStage.ToInput([new PipelineDecisionResolution("DEC-020", DecisionStage.ActionResolve, DecisionOutcome.NoTruthNeeded, null, "x")]));
        Assert.Contains(p1, p => p.Contains("aspect-markiert"));              // ohne Färbung: verboten

        var (ops, p2) = DecisionResolutionDerivation.Derive(core,
            DecisionStage.ToInput([new PipelineDecisionResolution("DEC-010", DecisionStage.ActionResolve, DecisionOutcome.NoTruthNeeded, null, "bewusst ohne Festlegung")]));
        Assert.Empty(p2);
        var plan = new DecisionResolutionPlanDocument(DecisionResolutionPlanDocument.CurrentSchemaVersion, "p1", T, ops);
        Assert.True(DecisionResolutionGate.Check(core, plan).Pass);

        var (resolved, report) = DecisionResolutionApply.Apply(core, plan, new HashSet<int> { 0 }, "run-y");
        var dec = resolved.Items.Single(i => i.ItemId == "DEC-010");
        Assert.False(dec.ReadStatus().IsOpenDecision);                        // geschlossen …
        Assert.Equal("true", dec.Metadata[DecisionAnswerMeta.WaivedKey]);     // … mit Verzichts-Stempel
        Assert.Contains("DEC-010", report.Resolved);
        Assert.True(CoreKangal.Check(resolved).Pass);
    }

    [Fact]
    public void Adapter_dritte_Option_NUR_bei_Architektur_Faerbung()
    {
        ReviewOptionsOf(aspect: DecisionAspectMeta.Architecture, out var mitFaerbung);
        ReviewOptionsOf(aspect: null, out var ohneFaerbung);
        Assert.Contains(mitFaerbung, o => o.Value == PipelineDecisionReviewAdapter.ChoiceNoTruth);
        Assert.DoesNotContain(ohneFaerbung, o => o.Value == PipelineDecisionReviewAdapter.ChoiceNoTruth);
        Assert.Equal(2, ohneFaerbung.Count);                                  // Neutralität: exakt die alte Palette
    }

    [Fact]
    public void Chat_Vokabular_traegt_die_per_Item_Palette_wie_die_UI()   // R-70
    {
        var arch = AgenticSdlc.Host.Steward.StewardGateVocabulary.ForDecision(targetless: true, aspect: DecisionAspectMeta.Architecture);
        Assert.Contains(arch, o => o.Code == "resolve/NO_TRUTH_NEEDED");
        var frage = AgenticSdlc.Host.Steward.StewardGateVocabulary.ForDecision(targetless: true, aspect: null);
        Assert.DoesNotContain(frage, o => o.Code == "resolve/NO_TRUTH_NEEDED");
        Assert.Equal(2, frage.Count);
        var widerspruch = AgenticSdlc.Host.Steward.StewardGateVocabulary.ForDecision();
        Assert.Equal(4, widerspruch.Count);                                   // Neutralität: alte Palette exakt
    }

    private static void ReviewOptionsOf(string? aspect, out IReadOnlyList<AgenticSdlc.HumanReview.ReviewOption> options)
    {
        var view = new PipelineDecisionItemView("DEC-001", "Frage?", null, "", [], "Frage?", "Offene Frage", Aspect: aspect);
        var s = PipelineDecisionReviewAdapter.BuildSession("r1", new PipelineDecisionReviewRequest("r1", [view]));
        options = s.Items[0].FieldOptions[PipelineDecisionReviewAdapter.FieldDecision];
    }

    // ── ④ §3-Projektion: die vier ehrlichen Ausgänge ──────────────────────────────────────────

    [Fact]
    public void GapSection_rendert_offen_warnt_bei_nacktem_Schliessen_und_schweigt_bei_Nachweis_oder_Verzicht()
    {
        var core = Doc(
            ArchDec("DEC-001", "Offen?"),
            ArchDec("DEC-002", "Nackt geschlossen?", open: false),
            ArchDec("DEC-003", "Mit Nachweis geschlossen?", open: false),
            ArchDec("DEC-004", "Bewusst verzichtet?", open: false,
                extraMeta: new Dictionary<string, string> { [DecisionAnswerMeta.WaivedKey] = "true" }),
            Item("DEC-005", "decision", "Keine Architektur-Frage", "open_decision"),
            Item("ARCH-50", "architecture", "Antwort auf DEC-003.", "accepted",
                new Dictionary<string, string> { [DecisionAnswerMeta.Key] = "DEC-003" }));

        var md = C4GapSection.Render(core);
        Assert.Contains("(DEC-001, offen)", md);
        Assert.Contains("⚠ geklärt ohne Architektur-Nachweis", md);
        Assert.Contains("DEC-002", md);
        Assert.DoesNotContain("DEC-003", md);                                 // Nachweis ⇒ sauber weg
        Assert.DoesNotContain("DEC-004", md);                                 // Verzicht ⇒ sauber weg
        Assert.DoesNotContain("DEC-005", md);                                 // ohne Färbung: nie hier
    }

    [Fact]
    public void Ensure_ersetzt_NUR_die_Sektion_und_ist_idempotent()
    {
        var root = Path.Combine(Path.GetTempPath(), "c4gap-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        try
        {
            var core = Doc(ArchDec("DEC-001", "Offen?"));
            C4GapSection.Ensure(root, core);                                  // ohne c4-Datei: No-op
            Assert.False(File.Exists(Path.Combine(root, "docs", "c4.md")));

            File.WriteAllText(Path.Combine(root, "docs", "c4.md"),
                "# Architektur-Landkarte (C4)\n\nDIAGRAMM-TEIL\n\n## Offene Architektur-Lücken\n\n- alter Agent-Text\n");
            C4GapSection.Ensure(root, core);
            var content = File.ReadAllText(Path.Combine(root, "docs", "c4.md"));
            Assert.Contains("DIAGRAMM-TEIL", content);                        // Redaktion unangetastet
            Assert.Contains("(DEC-001, offen)", content);
            Assert.DoesNotContain("alter Agent-Text", content);               // Sektion ersetzt
            var mtime = File.GetLastWriteTimeUtc(Path.Combine(root, "docs", "c4.md"));
            C4GapSection.Ensure(root, core);
            Assert.Equal(mtime, File.GetLastWriteTimeUtc(Path.Combine(root, "docs", "c4.md")));   // idempotent
        }
        finally { Directory.Delete(root, true); }
    }

    // ── ⑤ Wächter beidseitig + Wahrheits-Kanten im Input ─────────────────────────────────────

    [Fact]
    public void FrischeNotiz_misst_das_Delta_zum_Beleg_Stand_und_meldet_tote_Zitate()   // R-71
    {
        var root = Path.Combine(Path.GetTempPath(), "c4fresh2-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        var p = Path.Combine(root, "docs", "c4.md");
        try
        {
            // Ohne Stempel: Hinweis aufs erste Autor-Update — NIE der Voll-Bestand als „fehlt"-Liste.
            File.WriteAllText(p, "Kasten X — Beleg: ARCH-1");
            var core = Doc(Item("ARCH-1", "architecture", "Alt.", "superseded"), Item("ARCH-2", "architecture", "Neu."));
            var ohneStand = AuthoredDocDrafting.C4FrischeNotiz(root, core)!;
            Assert.Contains("Beleg-Stand", ohneStand);
            Assert.Contains("ABGELÖSTE", ohneStand);                          // tote Zitate immer gemeldet
            Assert.DoesNotContain("NEUE", ohneStand);

            // Mit Stempel (ARCH-1 gesehen): NUR das Delta (ARCH-2) wird gemeldet.
            File.WriteAllText(p, "Kasten X — Beleg: ARCH-1\n\n" + C4GapSection.Header + "\n"
                + C4GapSection.BelegStandPrefix + "ARCH-1 -->\n");
            var delta = AuthoredDocDrafting.C4FrischeNotiz(root, core)!;
            Assert.Contains("1 NEUE", delta);
            Assert.Contains("ARCH-2", delta);

            // Stempel deckt alles + kein totes Zitat ⇒ Ruhe.
            File.WriteAllText(p, "Kasten X — Beleg: ARCH-2\n\n" + C4GapSection.Header + "\n"
                + C4GapSection.BelegStandPrefix + "ARCH-1|ARCH-2 -->\n");
            var coreOhneTote = Doc(Item("ARCH-2", "architecture", "Neu."));
            Assert.Null(AuthoredDocDrafting.C4FrischeNotiz(root, coreOhneTote));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void GapSection_ziel_behaftete_Aufloesungen_bekommen_nie_ein_Warn()   // R-71
    {
        var core = new ProjectStateDocument("p", 4, T, [],
            [ArchDec("DEC-003", "Widerspruch zu REQ-70: Postgres.", open: false), Item("REQ-70", "requirement", "Ziel")],
            [new ProjectStateRelation("DEC-003", "REQ-70", "contradicts_resolved", "t", new Dictionary<string, string>())],
            [], []);
        Assert.DoesNotContain("DEC-003", C4GapSection.Render(core));          // Auflösungs-Maschine legitimiert

        // Zweite Ziel-Quelle der Derivation (targetEntityId-Metadatum) — gleiche Ausnahme, gleiche Semantik.
        var viaMeta = Doc(ArchDec("DEC-006", "Ziel via Metadatum.", open: false,
            extraMeta: new Dictionary<string, string> { ["targetEntityId"] = "REQ-1" }));
        Assert.DoesNotContain("DEC-006", C4GapSection.Render(viaMeta));
    }

    [Fact]
    public void BuildInput_c4_traegt_die_Wahrheits_Kanten()
    {
        var root = Path.Combine(Path.GetTempPath(), "c4in-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            var core = new ProjectStateDocument("p", 4, T, [],
                [Item("PBI-1", "pbi", "Arbeitspaket"), Item("ARCH-1", "architecture", "Rahmen.")],
                [new ProjectStateRelation("PBI-1", "ARCH-1", "constrained_by", "t", new Dictionary<string, string>())],
                [], []);
            var input = AuthoredDocDrafting.BuildInput(root, core, AuthoredDocument.Resolve("c4")!, null);
            Assert.Contains("WAHRHEITS-KANTEN", input);
            Assert.Contains("PBI-1", input);
            Assert.Contains("—constrained_by→ ARCH-1", input);
        }
        finally { Directory.Delete(root, true); }
    }
}

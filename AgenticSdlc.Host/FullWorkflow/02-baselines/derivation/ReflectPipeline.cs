using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// Geteilte, EINZIGE Quelle der Reflect-Logik (Task-Texte, Gate-Auswertung, Per-Runde-Snapshot, Item-Diff, Reports).
/// Beide Reflect-Formen nutzen sie → driftfrei (dieselbe Rolle wie <c>EvaluatorOutput</c> für die Jury): die
/// <b>node-interne</b> Schleife (<see cref="DerivationAgenticExecutor.HandleReflectAsync"/>, imperativer Loop, für
/// Mess-Isolation) UND die <b>edge-native</b> Form (<c>ReflectGraphWorkflow</c>: Producer→Gate→(conditional)→Revise/
/// Finalize, MAF-nativer Zyklus wie <c>CheckerRepairWorkflow</c>). Nur reine Funktionen + IO — kein Agent-Aufruf.
/// </summary>
internal static class ReflectPipeline
{
    /// <summary>Deterministisches Gate-Ergebnis: Befunde (fb_t Sammel), per-Item-Kritik, Judge-Verdikte, Anker-Defekte,
    /// Coverage, R1 (Treue-Verletzungsrate), Pass.</summary>
    internal sealed record GateOutcome(
        List<string> Fails,
        Dictionary<string, List<string>> FlaggedByItem,
        IReadOnlyList<InferenceVerdict> Verdicts,
        List<InvalidAnchor> Invalid,
        CoverageReport Coverage,
        double R1,
        bool GatePass);

    /// <summary>Der externe, unabhängige Gate: deterministische Anker-Validierung + Rechenschaft + unabhängiger
    /// Judge (contradicts/unrelated). IDENTISCH in beiden Formen.</summary>
    public static async Task<GateOutcome> EvaluateGateAsync(
        SourceArtifactSet sources, HashSet<string> sourceIds, ArtifactDocument doc, DerivationTools tools,
        InferenceChecker postHocChecker, CancellationToken ct)
    {
        var fails = new List<string>();
        var invalid = new List<InvalidAnchor>();
        var flaggedByItem = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        void Flag(string id, string reason) { if (!flaggedByItem.TryGetValue(id, out var l)) { l = []; flaggedByItem[id] = l; } l.Add(reason); }

        foreach (var it in doc.Items)
        {
            var anchors = it.SourceArtifactItemIds ?? [];
            var bad = anchors.Where(a => !sourceIds.Contains(a)).ToList();
            if (anchors.Count == 0) { invalid.Add(new InvalidAnchor(it.Text, anchors, bad, "MISSING_ANCHOR")); Flag(it.ItemId, "kein Anker gesetzt — verankere an einem realen Quell-Item oder verwirf begründet."); }
            else if (bad.Count > 0) { invalid.Add(new InvalidAnchor(it.Text, anchors, bad, "UNKNOWN_ANCHOR")); Flag(it.ItemId, $"unbekannte Anker: {string.Join(", ", bad)} — ersetze durch reale Quell-IDs."); }
        }
        var verdicts = (await postHocChecker.CheckAsync(doc.Items, sources.ItemsById(), ct).ConfigureAwait(false)).Verdicts;
        var cov = DerivationCoverage.Evaluate(sources, doc.Items, tools.AccountedItemIds, tools.Dismissals);
        var badV = verdicts.Where(v => v.Verdict is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated).ToList();
        foreach (var v in badV) Flag(v.ItemId, $"unabhängiger Prüfer: verdict={v.Verdict} — {v.Rationale}");
        var r1 = doc.Items.Count == 0 ? 0.0 : Math.Round((double)badV.Count / doc.Items.Count, 4);

        if (invalid.Count > 0) fails.Add($"{invalid.Count} ungültige Anker: {string.Join(", ", invalid.SelectMany(i => i.BadIds).Distinct())}.");
        if (badV.Count > 0) fails.Add($"{badV.Count} nicht-tragende Risiken (verdict∈contradicts/unrelated): {string.Join(", ", badV.Select(v => v.ItemId))}.");
        if (!cov.SelfAccountingClean) fails.Add($"Rechenschaft unsauber — unbehandelt: [{string.Join(", ", cov.UnaccountedItemIds)}], Kollision: [{string.Join(", ", cov.CollisionItemIds)}].");

        return new GateOutcome(fails, flaggedByItem, verdicts, invalid, cov, r1, fails.Count == 0);
    }

    public static string BuildAccountVerifyTask()
    {
        var t = new StringBuilder();
        t.AppendLine("Beginne. Deine Umwelt ist der verifizierte Projektzustand — sie wird dir NICHT vorab genannt.");
        t.AppendLine("Entdecke sie zuerst mit list_artifacts, konsultiere selbst, was du für dein Ziel brauchst, und speichere mit save_derived, wenn du genug Evidenz hast.");
        t.AppendLine("Prüfe deinen Entwurf VOR dem Speichern mit verify_derived und überarbeite schwache Items, bis deine Definition of Done erfüllt ist.");
        t.AppendLine("Rechenschaft: JEDES Quell-Item muss am Ende entweder Anker eines Risikos ODER via account_uncovered mit Grund verworfen sein. Begründe vor jeder Tool-Entscheidung kurz, warum.");
        t.AppendLine("Prüfe VOR dem Speichern mit check_accountability, ob etwas UNBEHANDELT ist oder du ein Item zugleich verankerst und verwirfst (Kollision); behebe beides selbst und speichere erst, wenn der Stand sauber ist.");
        return t.ToString();
    }

    // Self-Refine (arXiv:2303.17651): y_{t+1} = M(p_refine ∥ x ∥ y_t ∥ fb_t) — vorheriger Entwurf + per-Item-Feedback
    // explizit zurück; Erhalt-Anweisung = chirurgisch (abgenommene Items bleiben, nur beanstandete werden geändert).
    public static string BuildReviseTask(ArtifactDocument prevDraft, IReadOnlyDictionary<string, List<string>> flaggedByItem, CoverageReport cov)
    {
        var t = new StringBuilder();
        t.AppendLine("Dein vorheriger Entwurf wurde EXTERN und unabhängig geprüft und ist noch NICHT abgenommen.");
        t.AppendLine("Er ist noch gespeichert; dies ist dein AUSGANGSPUNKT — beginne NICHT bei null.");
        t.AppendLine();
        t.AppendLine("=== DEIN VORHERIGER ENTWURF (überarbeite genau diesen) ===");
        foreach (var it in prevDraft.Items)
        {
            var anchors = it.SourceArtifactItemIds is { Count: > 0 } a ? string.Join(", ", a) : "—";
            var flagged = flaggedByItem.TryGetValue(it.ItemId, out var reasons);
            t.AppendLine($"[{(flagged ? "BEANSTANDET" : "ABGENOMMEN")}] {it.ItemId}  (Anker: {anchors})");
            t.AppendLine($"    {it.Text}");
            if (flagged) foreach (var r in reasons!) t.AppendLine($"    → {r}");
        }
        t.AppendLine();
        if (!cov.SelfAccountingClean)
        {
            t.AppendLine("=== RECHENSCHAFT (noch unsauber) ===");
            if (cov.UnaccountedItemIds.Count > 0) t.AppendLine($"Unbehandelte Quell-Items (verankern ODER via account_uncovered begründet verwerfen): [{string.Join(", ", cov.UnaccountedItemIds)}]");
            if (cov.CollisionItemIds.Count > 0) t.AppendLine($"Kollision (zugleich verankert UND verworfen — mit account_uncovered dismiss=false zurücknehmen): [{string.Join(", ", cov.CollisionItemIds)}]");
            t.AppendLine();
        }
        t.AppendLine("=== AUFTRAG ===");
        t.AppendLine("Behalte die ABGENOMMENEN Items UNVERÄNDERT. Ändere/ersetze/verwirf NUR die BEANSTANDETEN Items (bessere tragende Anker, oder begründet verwerfen). Stelle lückenlose UND kollisionsfreie Rechenschaft her.");
        t.AppendLine("Entdecke bei Bedarf die Umwelt erneut mit list_artifacts, und speichere die vollständige finale Fassung (abgenommene + korrigierte Items) mit save_derived GENAU EINMAL.");
        return t.ToString();
    }

    public static string BuildNoDraftTask(IReadOnlyList<string> fails)
    {
        var t = new StringBuilder();
        t.AppendLine("Dein vorheriger Durchlauf hat KEIN Artefakt gespeichert. Befunde:");
        foreach (var f in fails) t.AppendLine($"- {f}");
        t.AppendLine("Führe die Ableitung erneut aus: entdecke die Umwelt mit list_artifacts, leite die tragenden Risiken ab, stelle lückenlose UND kollisionsfreie Rechenschaft her, und speichere mit save_derived GENAU EINMAL.");
        return t.ToString();
    }

    // Sichert den in dieser Runde gespeicherten Entwurf, bevor die nächste Runde die on-disk derived.json überschreibt.
    public static string SnapshotRound(RunContext run, string outDir, int round, string derivedPath)
    {
        var dest = DerivationLayout.File(outDir, DerivationLayout.Reflect, $"derived.round-{round:00}.json");
        File.Copy(derivedPath, dest, overwrite: true);
        return Path.GetRelativePath(run.RunDir, dest).Replace('\\', '/');
    }

    // Struktureller Item-Diff zwischen zwei gespeicherten Runden. Item-IDENTITÄT via normalisiertem Text (IDs sind
    // positionsbasiert); exakter Text-Match = strenger Stabilitäts-Test. Semantischer Zwilling von ARTIFACT_SUSPICIOUS_OVERWRITE.
    public static void EmitReviseDiff(RunContext run, string specId, string outDir, JsonSerializerOptions json,
        int fromRound, int toRound, ArtifactDocument prev, ArtifactDocument cur, string curSnapshotRel)
    {
        static string Key(ArtifactItem i) => i.Text.Trim();
        static string Anch(ArtifactItem i) => string.Join(",", (i.SourceArtifactItemIds ?? []).OrderBy(x => x, StringComparer.Ordinal));
        static string Short(string s) => s.Length <= 100 ? s : s[..100] + "…";

        var prevByText = prev.Items.GroupBy(Key).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var curByText = cur.Items.GroupBy(Key).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var kept = new List<string>(); var removed = new List<string>(); var added = new List<string>();
        var changedAnchors = new List<object>();
        foreach (var (text, pit) in prevByText)
        {
            if (curByText.TryGetValue(text, out var cit))
            {
                if (Anch(pit) == Anch(cit)) kept.Add(Short(text));
                else changedAnchors.Add(new { text = Short(text), from = Anch(pit), to = Anch(cit) });
            }
            else removed.Add(Short(text));
        }
        foreach (var (text, _) in curByText)
            if (!prevByText.ContainsKey(text)) added.Add(Short(text));

        var evt = new
        {
            type = "ARTIFACT_REVISED", runId = run.RunId, spec = specId, fromRound, toRound,
            prevCount = prev.Items.Count, curCount = cur.Items.Count,
            keptCount = kept.Count, removedCount = removed.Count, addedCount = added.Count, changedAnchorsCount = changedAnchors.Count,
            kept, removed, added, changedAnchors, snapshot = curSnapshotRel,
            note = "Item-Identität via normalisiertem Text (IDs sind positionsbasiert); exakter Text-Match = strenger Stabilitäts-Test.",
            timestampUtc = DateTime.UtcNow
        };
        run.AppendEvent(evt);
        File.WriteAllText(DerivationLayout.File(outDir, DerivationLayout.Reflect, $"revise.round-{fromRound:00}-to-{toRound:00}.json"), JsonSerializer.Serialize(evt, json));
    }

    /// <summary>Schreibt alle Reports (inference-check, dod, coverage, derivation-report inkl. reflect-Block). IDENTISCH
    /// in beiden Formen. Für Reflect gilt: wantsVerify=wantsCoverage=accountVerify=true, modeLabel=explore-account-verify-reflect.</summary>
    public static async Task WriteReportsAsync(
        ArtifactDocument doc, IReadOnlyList<InferenceVerdict> verdicts, IReadOnlyList<InvalidAnchor> invalid,
        SourceArtifactSet sources, DerivationTools tools, string decision, RunContext run, DerivationSpec spec, string outDir,
        string modeLabel, bool wantsVerify, bool wantsCoverage, bool accountVerify, bool independentPostHoc, string postHocJudgeModel,
        JsonSerializerOptions json, object? reflectBlock, CancellationToken ct)
    {
        var byVerdict = verdicts.GroupBy(v => v.Verdict).ToDictionary(g => g.Key.ToString(), g => g.Count());
        var report = new InferenceCheckReport(
            Pass: verdicts.All(v => v.Verdict is not (InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated)),
            Total: doc.Items.Count, ByVerdict: byVerdict, Verdicts: verdicts,
            Flagged: verdicts.Where(v => v.Verdict is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated).ToList());

        await File.WriteAllTextAsync(DerivationLayout.File(outDir, DerivationLayout.Checks, "inference-check-report.json"), JsonSerializer.Serialize(report, json), ct).ConfigureAwait(false);

        var used = doc.Items.SelectMany(i => i.SourceArtifactItemIds ?? []).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var metrics = DerivationMetrics.ComputeDeterministic(doc, sources, invalid, verdicts, doc.Items.Count);

        object? verifyBlock = null;
        if (wantsVerify)
        {
            var dod = DerivationDoD.Evaluate(doc, sources, verdicts);
            await File.WriteAllTextAsync(DerivationLayout.File(outDir, DerivationLayout.Checks, "dod-report.json"), JsonSerializer.Serialize(dod, json), ct).ConfigureAwait(false);
            var finalR1 = metrics.R1_FidelityViolationRate;
            verifyBlock = new
            {
                rounds = tools.VerifyRounds,
                firstDraftR1 = tools.FirstDraftR1,
                finalR1,
                deltaR1 = tools.FirstDraftR1 is double f ? Math.Round(f - finalR1, 4) : (double?)null,
                dodPass = dod.Pass, dodPassed = dod.Passed, dodTotal = dod.Total, dodFailures = dod.FailuresByCriterion
            };
        }

        object? coverageBlock = null;
        object? coverageDeltaBlock = null;
        if (wantsCoverage)
        {
            var cov = DerivationCoverage.Evaluate(sources, doc.Items, tools.AccountedItemIds, tools.Dismissals);
            await File.WriteAllTextAsync(DerivationLayout.File(outDir, DerivationLayout.Checks, "coverage-report.json"), JsonSerializer.Serialize(cov, json), ct).ConfigureAwait(false);
            coverageBlock = new
            {
                cov.Total, cov.Covered, cov.Accounted, cov.Unaccounted, cov.CoverageComplete,
                cov.DismissedRaw, cov.Collisions, cov.SelfAccountingClean,
                collisionRate = cov.Covered > 0 ? Math.Round((double)cov.Collisions / cov.Covered, 4) : 0.0,
                dismissalGroups = tools.Dismissals.Count
            };
            if (accountVerify)
                coverageDeltaBlock = new
                {
                    rounds = tools.AccountabilityRounds,
                    firstDraftUnaccounted = tools.FirstDraftUnaccounted,
                    finalUnaccounted = cov.Unaccounted,
                    deltaUnaccounted = tools.FirstDraftUnaccounted is int fu ? fu - cov.Unaccounted : (int?)null,
                    firstDraftCollisions = tools.FirstDraftCollisions,
                    finalCollisions = cov.Collisions,
                    deltaCollisions = tools.FirstDraftCollisions is int fc ? fc - cov.Collisions : (int?)null,
                    selfAccountingClean = cov.SelfAccountingClean
                };
        }

        await File.WriteAllTextAsync(Path.Combine(outDir, "derivation-report.json"), JsonSerializer.Serialize(new
        {
            spec = spec.Id, mode = modeLabel, decision,
            anchoredValid = doc.Items.Count - invalid.Count, invalidAnchor = invalid.Count, invalid,
            retrievedItemIds = tools.RetrievedItemIds.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            usedItemIds = used,
            retrievedCount = tools.RetrievedItemIds.Count, usedCount = used.Length,
            independentPostHoc, postHocJudgeModel,
            metrics, verify = verifyBlock, coverage = coverageBlock, coverageDelta = coverageDeltaBlock, reflect = reflectBlock
        }, json), ct).ConfigureAwait(false);
    }
}

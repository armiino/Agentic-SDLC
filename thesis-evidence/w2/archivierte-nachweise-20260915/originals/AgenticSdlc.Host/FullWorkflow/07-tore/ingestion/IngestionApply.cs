using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

public sealed record AppliedOperation(string IncomingItemId, string Kind, string? EntityId, string Outcome);

public sealed record IngestionDeltaSummary(int Added, int Refined, int Reaffirmed, int Superseded, int Contradicted, int AlreadyDecided, int Skipped, int Questions = 0);

public sealed record IngestionApplyReport(
    IReadOnlyList<AppliedOperation> Applied,
    IReadOnlyList<string> Skipped,
    IngestionDeltaSummary Delta);

// affected-view (Blast-Radius) lebt jetzt in CoreViews (Inc 1c-2) — transitiv ueber den Core-Graphen.

// Deterministischer Upsert-by-Identity der vom Menschen akzeptierten Operationen in den Core (kein LLM).
// Der Core ist die ID-Autoritaet: neue Entitaeten bekommen HIER eine stabile Core-ID (REQ-<max+1>).
// R-31/I7: jede INHALTS-Mutation traegt den AUSLOESER-Lauf (ingestRunId) als sourceRunId am Item; der
// Delta-Lauf des Incomings bleibt via Metadatum `ingestedFromRun` auffindbar. Reine Provenienz-Merges
// (RESTATE/ALREADY_DECIDED) lassen die Provenance unberuehrt — dieselbe Regel wie beim PbiUpdate-Apply.
public static class IngestionApply
{
    // A1a: profile = Aspekt-Naht (Default Requirement — Alt-Aufrufer/Tests unverändert; Graph reicht explizit).
    public static (ProjectStateDocument UpdatedCore, IngestionApplyReport Report, IReadOnlySet<string> AffectedIds) Apply(
        ProjectStateDocument core,
        ProjectStateDocument meetingDelta,
        StateChangePlanDocument plan,
        ISet<string> acceptedIncomingIds,
        string ingestRunId,
        AspectIngestionProfile? profile = null)
    {
        profile ??= AspectIngestionProfile.Requirement;
        var order = core.Items.Select(i => i.ItemId).ToList();
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var relations = core.Relations.ToList();
        var incomingById = meetingDelta.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        var nextReq = MaxSuffix(order, profile.IdPrefix) + 1;
        var nextDec = MaxSuffix(order, "DEC") + 1;

        var applied = new List<AppliedOperation>();
        var skipped = new List<string>();
        var affected = new HashSet<string>(StringComparer.Ordinal);
        int added = 0, refined = 0, reaffirmed = 0, superseded = 0, contradicted = 0, alreadyDecided = 0, questions = 0;

        foreach (var op in plan.Operations.Where(o => acceptedIncomingIds.Contains(o.IncomingItemId)))
        {
            if (!incomingById.TryGetValue(op.IncomingItemId, out var incoming))
            {
                skipped.Add($"{op.IncomingItemId}: incoming Item nicht im MeetingDelta");
                continue;
            }

            var claimIds = Union(op.ClaimIds, incoming.SourceClaimIds);
            var statement = string.IsNullOrWhiteSpace(op.Statement) ? incoming.Text : op.Statement;

            switch (op.Kind)
            {
                case StateChangeKind.Restate:
                {
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var t)) { skipped.Add($"{op.IncomingItemId}: RESTATE-Ziel unbekannt"); break; }
                    byId[t.ItemId] = t with { SourceClaimIds = Union(t.SourceClaimIds, op.ClaimIds) };
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, t.ItemId, "reaffirmed"));
                    affected.Add(t.ItemId); reaffirmed++;
                    break;
                }
                case StateChangeKind.Refine:
                {
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var t)) { skipped.Add($"{op.IncomingItemId}: REFINE-Ziel unbekannt"); break; }
                    var history = (t.History ?? []).Append(new ProjectStateItemVersion(
                        t.Version, t.Text, t.Status, t.Origin, t.SourceRunId, t.SourceClaimIds, DateTime.UtcNow,
                        $"REFINE (ingestion {ingestRunId}, via {op.IncomingItemId})")).ToList();
                    // Z4-Fix (20.08., Abnahme-Fund): auch REFINE merged die Herkunfts-Metadata des Incomings
                    // (GitHub-Anker, Analyst-Kategorie/Herleitung) — vorher behielt die neue Version NUR die
                    // Ziel-Metadata und analystKategorie starb still (NFR-Sektion wäre leer geblieben).
                    var refineMeta = new Dictionary<string, string>(t.Metadata, StringComparer.Ordinal);
                    GithubOriginMeta.CarryOver(incoming, refineMeta);
                    AnalystOriginMeta.CarryOver(incoming, refineMeta);
                    // R-74b: auch eine REFINE-Antwort trägt ihren Anker — verfeinert ein Antwort-Diktat
                    // bestehende Wahrheit, ist DIE das answering-Item (sonst stürbe der Nachweis hier still).
                    DecisionAnswerMeta.CarryOver(incoming, refineMeta);
                    byId[t.ItemId] = t with
                    {
                        Text = statement,
                        Version = t.Version + 1,
                        IdentityKey = IdentityKey.From(statement),
                        SourceRunId = ingestRunId,
                        SourceClaimIds = claimIds,
                        Metadata = refineMeta,
                        History = history
                    };
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, t.ItemId, "refined"));
                    affected.Add(t.ItemId); refined++;
                    break;
                }
                case StateChangeKind.New:
                case StateChangeKind.NewRelated:
                {
                    var id = $"{profile.IdPrefix}-{nextReq++:D2}";
                    var meta = IngestMeta(op.IncomingItemId, incoming);
                    // R-36 v2: featureKey ist ein reines HINWEIS-Metadatum fuer die Placement-Stufe — hier entsteht
                    // KEINE part_of_feature-Relation mehr. Die REQ→Feature-Kante wird deterministisch im
                    // pbi-update-Apply aus der bestaetigten Deckung abgeleitet (Seed-Regel, CoreBacklogSeeder).
                    // (Der alte Direkt-Write hier schrieb Agent-Freitext als Relationsziel = die 12 Defekte des Audits.)
                    if (op.Kind == StateChangeKind.NewRelated && !string.IsNullOrWhiteSpace(op.FeatureKey))
                        meta["featureKey"] = op.FeatureKey!;
                    AddItem(order, byId, NewRequirement(id, statement, incoming, claimIds, meta, ingestRunId, profile));
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, id, "added"));
                    affected.Add(id); added++;
                    break;
                }
                case StateChangeKind.Supersede:
                {
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var t)) { skipped.Add($"{op.IncomingItemId}: SUPERSEDE-Ziel unbekannt"); break; }
                    // §5-S3: Status über die zentrale Naht (Alt-String + neue Felder synchron) + History-Notiz (schließt E-10-Lücke).
                    byId[t.ItemId] = t.WithStatus(CoreStatus.From("superseded"), $"superseded via Ingestion-SUPERSEDE ({op.IncomingItemId}, {ingestRunId})");
                    var id = $"{profile.IdPrefix}-{nextReq++:D2}";
                    AddItem(order, byId, NewRequirement(id, statement, incoming, claimIds, IngestMeta(op.IncomingItemId, incoming), ingestRunId, profile));
                    relations.Add(new ProjectStateRelation(id, t.ItemId, "supersedes", "ingestion", new Dictionary<string, string>()));
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, id, "superseded"));
                    affected.Add(id); affected.Add(t.ItemId); superseded++;
                    break;
                }
                case StateChangeKind.Contradict:
                {
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var t)) { skipped.Add($"{op.IncomingItemId}: CONTRADICT-Ziel unbekannt"); break; }
                    var id = $"DEC-{nextDec++:D3}";
                    var meta = IngestMeta(op.IncomingItemId, incoming);
                    meta[DecisionTargetMeta.Key] = t.ItemId;
                    AddItem(order, byId, new ProjectStateItem(
                        ItemId: id, ItemType: "decision", Text: $"Widerspruch zu {t.ItemId}: {statement}",
                        Origin: "INGESTION_CONTRADICTION", Stage: null, Version: 1, SourceRunId: ingestRunId,
                        SourceArtifactId: null, SourceArtifactType: null, SourceDecisionId: null, SourceCandidateId: null,
                        SourceClaimIds: claimIds, SourceArtifactItemIds: [], Metadata: meta,
                        IdentityKey: IdentityKey.From(statement), History: []).WithStatus(CoreStatus.From("open_decision")));   // §5-S7: Status→Achsen
                    relations.Add(new ProjectStateRelation(id, t.ItemId, "contradicts", "ingestion", new Dictionary<string, string>()));
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, id, "contradicted"));
                    affected.Add(id); affected.Add(t.ItemId); contradicted++;
                    break;
                }
                case StateChangeKind.OpenQuestion:
                {
                    // 9g: die im Meeting gestellte Frage wird zur offenen Entscheidung — geteilter Mint
                    // (dieselbe Semantik nutzt der Bootstrap), KEINE Relation, kein Block, DEC-Muenze wie CONTRADICT.
                    var id = $"DEC-{nextDec++:D3}";
                    AddItem(order, byId, MeetingQuestionMint.NewDecision(id, statement, incoming, claimIds, ingestRunId));
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, id, "question_opened"));
                    affected.Add(id); questions++;
                    break;
                }
                case StateChangeKind.AlreadyDecided:
                {
                    // incoming ist bereits als Open Decision erfasst -> nur Provenienz/Claims an die DEC anheften (No-Op).
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var dec) || !string.Equals(dec.ItemType, "decision", StringComparison.OrdinalIgnoreCase))
                    { skipped.Add($"{op.IncomingItemId}: ALREADY_DECIDED-Ziel ist keine Open Decision"); break; }
                    byId[dec.ItemId] = dec with { SourceClaimIds = Union(dec.SourceClaimIds, op.ClaimIds) };
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, dec.ItemId, "already_decided"));
                    affected.Add(dec.ItemId); alreadyDecided++;
                    break;
                }
                default:
                    skipped.Add($"{op.IncomingItemId}: unbekannte Operation {op.Kind}");
                    break;
            }
        }

        var items = order.Select(id => byId[id]).ToList();
        var updated = core with
        {
            SchemaVersion = ProjectStateDocument.CurrentSchemaVersion,
            Items = items,
            Relations = relations
        };
        var report = new IngestionApplyReport(applied, skipped,
            new IngestionDeltaSummary(added, refined, reaffirmed, superseded, contradicted, alreadyDecided, skipped.Count, questions));
        return (updated, report, affected);
    }

    // R-31/I7: sourceRunId = Ausloeser-Lauf (Ingest); Herkunft des Incomings steckt in Metadata (IngestMeta).
    private static ProjectStateItem NewRequirement(string id, string text, ProjectStateItem incoming, IReadOnlyList<string> claimIds, Dictionary<string, string> meta, string ingestRunId, AspectIngestionProfile profile)
        => new ProjectStateItem(
            ItemId: id, ItemType: profile.Aspect, Text: text, Origin: incoming.Origin,
            Stage: incoming.Stage, Version: 1, SourceRunId: ingestRunId, SourceArtifactId: incoming.SourceArtifactId,
            SourceArtifactType: incoming.SourceArtifactType, SourceDecisionId: null, SourceCandidateId: null,
            SourceClaimIds: claimIds, SourceArtifactItemIds: incoming.SourceArtifactItemIds, Metadata: meta,
            IdentityKey: IdentityKey.From(text), History: []).WithStatus(CoreStatus.From("accepted"));   // §5-S7: Status→Achsen

    // ingestedFrom = Incoming-ID im MeetingDelta; ingestedFromRun = der Lauf, der das Delta produzierte
    // (R-31: BEIDE Herkuenfte am Item auffindbar, ohne den sourceRunId-Platz des Ausloeser-Laufs zu belegen).
    // 9i/9m: GitHub-Herkunft (Issue-Nr/Hashes) reist MIT ins Core-Item — die Wahrheit selbst ist das
    // Ernte-Gedaechtnis und die deterministische Forward-Link-Quelle (no-op fuer Meeting-/Autor-Incomings).
    private static Dictionary<string, string> IngestMeta(string incomingItemId, ProjectStateItem incoming)
    {
        var meta = new Dictionary<string, string>(StringComparer.Ordinal) { ["ingestedFrom"] = incomingItemId };
        if (!string.IsNullOrWhiteSpace(incoming.SourceRunId)) meta["ingestedFromRun"] = incoming.SourceRunId!;
        GithubOriginMeta.CarryOver(incoming, meta);
        // 1g: Analyst-Herkunft (Kategorie/Herleitung/Linse) reist MIT in die Wahrheit — Kategorie strukturiert
        // das Anforderungsdokument, Herleitung+Linse sind der Beleg (no-op für andere Bahnen).
        AnalystOriginMeta.CarryOver(incoming, meta);
        // C4-Kreislauf (22.08.): der Antwort-Anker („dieses Item beantwortet DEC-x") reist in die Wahrheit —
        // damit ist die Schließung einer Architektur-Unklarheit deterministisch nachweisbar (§3-Projektion).
        DecisionAnswerMeta.CarryOver(incoming, meta);
        return meta;
    }

    private static void AddItem(List<string> order, Dictionary<string, ProjectStateItem> byId, ProjectStateItem item)
    {
        byId[item.ItemId] = item;
        order.Add(item.ItemId);
    }

    private static int MaxSuffix(IEnumerable<string> ids, string prefix)
    {
        var p = prefix + "-";
        return ids.Where(id => id.StartsWith(p, StringComparison.Ordinal))
            .Select(id => id[p.Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();
    }

    // R-19: null-tolerant — Deltas ohne Claim-Provenance (sourceClaimIds=null) sind schema-legal und
    // dürfen den Apply nicht mit ArgumentNullException töten (Fund: Mini-Meeting-3, Run 150814).
    private static IReadOnlyList<string> Union(IEnumerable<string>? a, IEnumerable<string>? b)
        => (a ?? []).Concat(b ?? []).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
}

using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// A10: hebt die per Adjudikation NEU geminteten Claims (facetStatus=pending) auf Pipeline-Niveau.
///   ledger-adjudicate-refine &lt;consumable.json&gt; [transcript.txt] [model] [out.json]
/// Das Filtern der pending-Claims ist DETERMINISTISCH (keine LLM-Kosten für bereits facettierte Claims);
/// nur die pending-Claims gehen an den <see cref="FacetAssigner"/>. Alles andere läuft unverändert durch.
/// </summary>
public static class LedgerAdjudicateRefineRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: ledger-adjudicate-refine <consumable.json> [transcript.txt] [model] [out.json]");
            return 2;
        }
        var consumablePath = Resolve(repoRoot, args[1]);
        if (!File.Exists(consumablePath)) { Console.Error.WriteLine($"[refine] consumable fehlt: {consumablePath}"); return 2; }

        var transcript = args.Length >= 3 && File.Exists(Resolve(repoRoot, args[2]))
            ? await File.ReadAllTextAsync(Resolve(repoRoot, args[2])).ConfigureAwait(false)
            : null;
        var modelArg = args.Length >= 4 ? args[3] : null;
        var outPath = args.Length >= 5 ? Resolve(repoRoot, args[4]) : consumablePath; // Default: in-place.

        var consumable = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consumablePath), Json);
        if (consumable is null) { Console.Error.WriteLine("[refine] consumable nicht lesbar."); return 2; }
        var claims = consumable.Claims.ToList();

        // DETERMINISTISCHER Filter: nur pending-Claims (keine LLM-Kosten für den Rest).
        var pendingIdx = claims.Select((c, i) => (c, i))
            .Where(x => string.Equals(x.c.FacetStatus, "pending", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine($"[refine] consumable claims={claims.Count} pending={pendingIdx.Count}");
        if (pendingIdx.Count == 0)
        {
            Console.WriteLine("[refine] nichts zu tun (keine facetStatus=pending Claims) — kein LLM-Call.");
            return 0;
        }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        Console.WriteLine($"[refine] Facetten-Zuweisung für {pendingIdx.Count} Claim(s) mit model={judgeSettings.ModelId} (transcript={(transcript is null ? "nein" : "ja")})");

        var assigner = new FacetAssigner(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);
        IReadOnlyList<SemanticLedgerEntry> refined;
        try
        {
            refined = await assigner.AssignAllAsync(pendingIdx.Select(x => x.c).ToList(), transcript, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[refine] Facetten-Zuweisung fehlgeschlagen: {ex.Message}");
            return 4;
        }

        var refinedById = refined.ToDictionary(r => r.Id, r => r, StringComparer.Ordinal);
        foreach (var (c, i) in pendingIdx)
            if (refinedById.TryGetValue(c.Id, out var r)) claims[i] = r;

        var stillPending = claims.Count(c => string.Equals(c.FacetStatus, "pending", StringComparison.OrdinalIgnoreCase));
        var updated = consumable with
        {
            Claims = claims,
            Note = consumable.Note + " | A10-refine: neue Claims facettiert (facetStatus gelöscht).",
        };
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(updated, Json)).ConfigureAwait(false);

        Console.WriteLine($"[refine] refined={refined.Count} nochPending={stillPending} -> {Path.GetRelativePath(repoRoot, outPath)}");
        foreach (var r in refined)
            Console.WriteLine($"[refine]   {r.Id}: kind={r.Kind} status={r.Status} modality={r.Modality} timeScope={r.TimeScope}");
        return 0;
    }

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}

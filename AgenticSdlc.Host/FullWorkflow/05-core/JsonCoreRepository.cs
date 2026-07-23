using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// JSON-Implementierung des Core-Ports. Nutzt dieselbe Serialisierung wie der ProjectState
// (ProjectStateJson.Options) - EIN Modell, EIN Format. DB-Adapter spaeter hinter demselben Interface.
public sealed class JsonCoreRepository(string repoRoot) : ICoreRepository
{
    private readonly string _file = CorePaths.CoreFile(repoRoot);

    public Task<bool> ExistsAsync(CancellationToken ct = default) => Task.FromResult(File.Exists(_file));

    public async Task<ProjectStateDocument> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_file))
            throw new InvalidOperationException($"CORE_NOT_FOUND: {_file} - erst 'core-seed' fahren.");
        var json = await File.ReadAllTextAsync(_file, ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<ProjectStateDocument>(json, ProjectStateJson.Options)
               ?? throw new InvalidOperationException($"CORE_UNREADABLE: {_file}");
    }

    public async Task SaveAsync(ProjectStateDocument core, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_file) ?? ".");
        var json = JsonSerializer.Serialize(core, ProjectStateJson.Options);
        await SnapshotBeforeOverwriteAsync(json, ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(_file, json, ct).ConfigureAwait(false);
    }

    // Wiederherstell-Schutz: der bisherige Stand wandert vor dem Ueberschreiben nach state/core/history/
    // (nur bei echter Aenderung — 0-Ops-Applies erzeugen keine identischen Kopien). Restore = Snapshot zurueckkopieren.
    private async Task SnapshotBeforeOverwriteAsync(string newJson, CancellationToken ct)
    {
        if (!File.Exists(_file)) return;
        var current = await File.ReadAllTextAsync(_file, ct).ConfigureAwait(false);
        if (string.Equals(current, newJson, StringComparison.Ordinal)) return;

        var historyDir = Path.Combine(Path.GetDirectoryName(_file)!, "history");
        Directory.CreateDirectory(historyDir);
        var snapshot = Path.Combine(historyDir, $"project-state.{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.json");
        await File.WriteAllTextAsync(snapshot, current, ct).ConfigureAwait(false);
    }
}

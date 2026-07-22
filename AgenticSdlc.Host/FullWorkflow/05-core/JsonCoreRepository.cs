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
        await File.WriteAllTextAsync(_file, JsonSerializer.Serialize(core, ProjectStateJson.Options), ct).ConfigureAwait(false);
    }
}

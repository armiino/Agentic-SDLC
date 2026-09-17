using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using System.Text.Json;
using System.Security.Cryptography;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

public sealed class TranscriptSpeakerRegressionTests
{
    [Fact]
    public void Hyphenated_role_starts_its_own_turn_and_does_not_inherit_previous_speaker()
    {
        const string text = "Leitung: Gut, dann Notiz mit Push.\n\n" +
            "Angehörigen-Vertreterin: Zum Schluss noch ein Wunsch: Besuchsplanung.\n\n" +
            "Leitung: Das ist hilfreich.";
        var turns = TranscriptSegmenter.Segment(text);
        Assert.Equal(3, turns.Count);
        Assert.Equal(new[] { "Leitung", "Angehörigen-Vertreterin", "Leitung" }, turns.Select(t => t.Speaker));
        Assert.Equal("Gut, dann Notiz mit Push.", turns[0].Text);
        Assert.Equal("Zum Schluss noch ein Wunsch: Besuchsplanung.", turns[1].Text);
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "input", "transcripts")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Isolated input snapshot missing.");
    }

    [Fact]
    public void Historical_F1_source_has_separate_visit_planning_unit_with_correct_speaker()
    {
        const string source = "meeting-4-ux-block-l.txt";
        var text = File.ReadAllText(Path.Combine(RepoRoot(), "input", "transcripts", source));
        var units = AtomicUnitSegmenter.Segment(text, source);
        Assert.Equal(17, units.Count);
        Assert.Equal(4, units.Count(u => u.Speaker == "Angehörigen-Vertreterin"));
        var visit = Assert.Single(units, u => u.Text.Contains("Zum Schluss das zweite neue Thema"));
        Assert.Equal("Angehörigen-Vertreterin", visit.Speaker);
        Assert.Equal("AU-0016", visit.Id);
        Assert.DoesNotContain("Angehörigen-Vertreterin:", visit.Text);
    }

    [Fact]
    public void Corpus_comparison_records_changes_and_preserves_both_W2_inputs_exactly()
    {
        var root = RepoRoot();
        var rows = new List<object>();
        var changed = new List<string>();
        foreach (var path in Directory.GetFiles(Path.Combine(root, "input", "transcripts"), "*.txt").Order())
        {
            var name = Path.GetFileName(path);
            var text = File.ReadAllText(path);
            var before = TranscriptSegmenterBeforeB20.Segment(text)
                .Select((t, i) => new AtomicUnit($"AU-{i + 1:D4}", name, t.Index, t.Speaker, t.Text)).ToList();
            var after = AtomicUnitSegmenter.Segment(text, name);
            var beforeBytes = JsonSerializer.SerializeToUtf8Bytes(before);
            var afterBytes = JsonSerializer.SerializeToUtf8Bytes(after);
            bool identical = beforeBytes.SequenceEqual(afterBytes);
            if (!identical) changed.Add(name);
            rows.Add(new { source = name, sourceSha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))),
                beforeCount = before.Count, afterCount = after.Count, identical,
                beforeUnitsSha256 = Convert.ToHexStringLower(SHA256.HashData(beforeBytes)),
                afterUnitsSha256 = Convert.ToHexStringLower(SHA256.HashData(afterBytes)) });
        }
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "b20-corpus-comparison.json"),
            JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
        Assert.Equal(new[] { "meeting-4-ux-block-l.txt" }, changed);
    }
}

using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

public sealed record AtomicUnitFixture(
    [property: JsonPropertyName("units")] IReadOnlyList<AtomicUnit> Units);

public sealed record AtomicUnit(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("turnIndex")] int TurnIndex,
    [property: JsonPropertyName("speaker")] string Speaker,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("unitType")] string UnitType = "turn");

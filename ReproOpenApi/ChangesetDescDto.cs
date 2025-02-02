using System.Text.Json.Serialization;

namespace ReproOpenApi;

public sealed class ChangesetDescDto
{
    [JsonPropertyName("value1")]
    public required ChangesetIntDto Prop1 { get; init; }

    [JsonPropertyName("value2")]
    public required ChangesetIntDto Prop2 { get; init; }
}

public sealed class ChangesetIntDto
{
    [JsonPropertyName("added")]
    public required List<int> Added { get; init; }
}

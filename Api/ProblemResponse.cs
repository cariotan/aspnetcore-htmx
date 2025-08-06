using System.Text.Json.Serialization;

public class ProblemResponse
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("status")]
    public required int Status { get; set; }

    [JsonPropertyName("detail")]
    public required string Detail { get; set; }

    [JsonPropertyName("traceId")]
    public required string TraceId { get; set; }
}
using System.Text.Json;

public record DocumentExtractedMessage
{
    Guid DocumentId { get; set; }
    JsonDocument ParsedData { get; set; }
}
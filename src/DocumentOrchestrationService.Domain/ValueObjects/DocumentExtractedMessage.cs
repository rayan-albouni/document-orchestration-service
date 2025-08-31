using System.Text.Json;

public record DocumentExtractedMessage
{
    public Guid DocumentId { get; set; }
    public required string TenantId { get; set; }
    public required string ParsedData { get; set; } 
}
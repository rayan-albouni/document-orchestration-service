public record DocumentClassifiedMessage
{
    public Guid DocumentId { get; set; }
    public required string TenantId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string Status { get; set; } = string.Empty;
}

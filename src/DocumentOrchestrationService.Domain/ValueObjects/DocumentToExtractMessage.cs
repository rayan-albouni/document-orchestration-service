public record DocumentToExtractMessage
{
    public Guid DocumentId { get; set; }
    public required string TenantId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
}

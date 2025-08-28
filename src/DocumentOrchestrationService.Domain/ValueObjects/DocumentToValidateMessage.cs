public record DocumentToValidateMessage
{
    public Guid DocumentId { get; set; }
    public required string TenantId { get; set; }
    public required string BlobUrl { get; set; }
}
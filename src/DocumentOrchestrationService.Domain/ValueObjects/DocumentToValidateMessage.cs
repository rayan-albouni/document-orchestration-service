public record DocumentToValidateMessage
{
    public Guid DocumentId { get; set; }
    public required string TenantId { get; set; }
    public required string DocumentType { get; set; }
    public required string ParsedData { get; set; }
}
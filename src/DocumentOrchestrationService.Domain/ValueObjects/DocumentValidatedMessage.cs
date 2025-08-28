public record DocumentValidatedMessage
{
    public Guid DocumentId { get; set; }
    public required string TenantId { get; set; } 
    public string ValidationResult { get; set; } = string.Empty;
    public bool RequiresReview { get; set; }
}
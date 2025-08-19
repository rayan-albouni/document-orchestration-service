namespace DocumentOrchestrationService.Domain.Entities;

public class ProcessingJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public ProcessingStatus OverallStatus { get; set; } = ProcessingStatus.Processing;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? ClassificationResult { get; set; }
    public string? ExtractionResult { get; set; }
    public string? ValidationResult { get; set; }
    public bool RequiresHumanReview { get; set; }
    public string? HumanReviewTaskId { get; set; }
    public string? ProcessedDataId { get; set; }
    public string? ErrorMessage { get; set; }
}

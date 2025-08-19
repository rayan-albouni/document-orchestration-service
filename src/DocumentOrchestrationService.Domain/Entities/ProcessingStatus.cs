namespace DocumentOrchestrationService.Domain.Entities;

public enum ProcessingStatus
{
    Processing,
    Classified,
    Extracted,
    Validated,
    PendingHumanReview,
    Reviewed,
    Completed,
    Failed
}

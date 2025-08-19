namespace DocumentOrchestrationService.Domain.ValueObjects;

public record DocumentStatusResponse(
    string DocumentId,
    string TenantId,
    string OverallStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool RequiresHumanReview,
    string? HumanReviewTaskId,
    string? ErrorMessage
);

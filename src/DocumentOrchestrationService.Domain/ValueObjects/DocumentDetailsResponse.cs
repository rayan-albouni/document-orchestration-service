namespace DocumentOrchestrationService.Domain.ValueObjects;

public record DocumentDetailsResponse(
    string DocumentId,
    string TenantId,
    string BlobUrl,
    string? DocumentType,
    string SourceSystem,
    string UserId,
    string ClientReferenceId,
    string OverallStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? ClassificationResult,
    double? ClassificationConfidenceScore,
    string? ExtractionResult,
    string? ValidationResult,
    bool RequiresHumanReview,
    string? HumanReviewTaskId,
    string? ProcessedDataId,
    string? ErrorMessage
);
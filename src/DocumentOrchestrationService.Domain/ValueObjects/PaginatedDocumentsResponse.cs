using DocumentOrchestrationService.Domain.Entities;

namespace DocumentOrchestrationService.Domain.ValueObjects;

public record DocumentSummary(
    string DocumentId,
    string TenantId,
    string? DocumentType,
    string SourceSystem,
    string UserId,
    string ClientReferenceId,
    ProcessingStatus OverallStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool RequiresHumanReview,
    string? ErrorMessage,
    string BlobUrl,
    string? ClassificationResult
);

public record PaginatedDocumentsResponse(
    List<DocumentSummary> Documents,
    int PageCount,
    int PageSize,
    string? ContinuationToken,
    bool HasMoreResults
);

public record DocumentsQueryRequest(
    string TenantId,
    int PageSize = 20,
    string? ContinuationToken = null,
    string? SortBy = "CreatedAt",
    string? SortOrder = "desc"
);

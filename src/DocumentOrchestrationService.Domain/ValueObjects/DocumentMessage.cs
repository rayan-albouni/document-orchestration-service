namespace DocumentOrchestrationService.Domain.ValueObjects;

public record DocumentMessage(
    string DocumentId,
    string TenantId,
    string FileName,
    string ContentType,
    long FileSize,
    string StorageLocation
);

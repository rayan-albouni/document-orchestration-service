namespace DocumentOrchestrationService.Domain.ValueObjects;

public record DocumentMetadata(
    string? DocumentType,
    string SourceSystem,
    string UserId,
    string TenantId,
    string ClientReferenceId
);

public record DocumentMessage(
    string DocumentId,
    string BlobUrl,
    DocumentMetadata Metadata
)
{
    // Convenience properties for easier access
    public string TenantId => Metadata.TenantId;
    public string UserId => Metadata.UserId;
    public string? DocumentType => Metadata.DocumentType;
    public string SourceSystem => Metadata.SourceSystem;
    public string ClientReferenceId => Metadata.ClientReferenceId;
};

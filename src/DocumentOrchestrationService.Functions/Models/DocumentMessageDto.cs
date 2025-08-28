using Newtonsoft.Json;
using DocumentOrchestrationService.Domain.ValueObjects;

namespace DocumentOrchestrationService.Functions.Models;

public class DocumentMetadataDto
{
    [JsonProperty("DocumentType")]
    public string? DocumentType { get; set; }
    
    [JsonProperty("SourceSystem")]
    public string SourceSystem { get; set; } = string.Empty;
    
    [JsonProperty("UserId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("TenantId")]
    public string TenantId { get; set; } = string.Empty;
    
    [JsonProperty("ClientReferenceId")]
    public string ClientReferenceId { get; set; } = string.Empty;
}

public class DocumentMessageDto
{
    [JsonProperty("documentId")]
    public Guid DocumentId { get; set; }
    
    [JsonProperty("blobUrl")]
    public string BlobUrl { get; set; } = string.Empty;
    
    [JsonProperty("metadata")]
    public DocumentMetadataDto Metadata { get; set; } = new();

    public DocumentIngestedMessage ToDomainObject()
    {
        var metadata = new DocumentMetadata(
            Metadata.DocumentType,
            Metadata.SourceSystem,
            Metadata.UserId,
            Metadata.TenantId,
            Metadata.ClientReferenceId
        );

        return new DocumentIngestedMessage(
            DocumentId,
            BlobUrl,
            metadata
        );
    }
}

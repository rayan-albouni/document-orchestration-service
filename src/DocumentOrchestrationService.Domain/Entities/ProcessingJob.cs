using System.Text.Json.Serialization;

namespace DocumentOrchestrationService.Domain.Entities;

public class ProcessingJob
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = string.Empty;
    
    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = string.Empty;
    
    [JsonPropertyName("blobUrl")]
    public string BlobUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("documentType")]
    public string? DocumentType { get; set; }
    
    [JsonPropertyName("sourceSystem")]
    public string SourceSystem { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("clientReferenceId")]
    public string ClientReferenceId { get; set; } = string.Empty;
    
    [JsonPropertyName("overallStatus")]
    public ProcessingStatus OverallStatus { get; set; } = ProcessingStatus.Processing;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("classificationResult")]
    public string? ClassificationResult { get; set; }
    
    [JsonPropertyName("extractionResult")]
    public string? ExtractionResult { get; set; }
    
    [JsonPropertyName("validationResult")]
    public string? ValidationResult { get; set; }
    
    [JsonPropertyName("requiresHumanReview")]
    public bool RequiresHumanReview { get; set; }
    
    [JsonPropertyName("humanReviewTaskId")]
    public string? HumanReviewTaskId { get; set; }
    
    [JsonPropertyName("processedDataId")]
    public string? ProcessedDataId { get; set; }
    
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

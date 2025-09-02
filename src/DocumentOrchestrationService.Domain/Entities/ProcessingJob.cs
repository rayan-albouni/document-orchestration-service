using Newtonsoft.Json;

namespace DocumentOrchestrationService.Domain.Entities;

public class ProcessingJob
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty(PropertyName = "documentId")]
    public string DocumentId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "tenantId")]
    public string TenantId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "blobUrl")]
    public string BlobUrl { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "documentType")]
    public string? DocumentType { get; set; }

    [JsonProperty(PropertyName = "sourceSystem")]
    public string SourceSystem { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "clientReferenceId")]
    public string ClientReferenceId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "overallStatus")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public ProcessingStatus OverallStatus { get; set; } = ProcessingStatus.Processing;

    [JsonProperty(PropertyName = "createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty(PropertyName = "updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty(PropertyName = "classificationResult")]
    public string? ClassificationResult { get; set; }

    [JsonProperty(PropertyName = "classificationConfidenceScore")]
    public double? ClassificationConfidenceScore { get; set; }

    [JsonProperty(PropertyName = "extractionResult")]
    public string? ExtractionResult { get; set; }

    [JsonProperty(PropertyName = "validationResult")]
    public string? ValidationResult { get; set; }

    [JsonProperty(PropertyName = "requiresHumanReview")]
    public bool RequiresHumanReview { get; set; }

    [JsonProperty(PropertyName = "humanReviewTaskId")]
    public string? HumanReviewTaskId { get; set; }

    [JsonProperty(PropertyName = "processedDataId")]
    public string? ProcessedDataId { get; set; }

    [JsonProperty(PropertyName = "errorMessage")]
    public string? ErrorMessage { get; set; }
}

namespace DocumentOrchestrationService.Domain.Constants;

public static class ServiceBusQueues
{
    public const string DocumentIngestionQueue = "document-ingestion-queue";
    public const string DocumentClassificationQueue = "document-classification-queue";
    public const string DocumentClassificationResultsQueue = "document-classification-results";
    public const string DocumentExtractionQueue = "document-extraction-queue";
    public const string DocumentExtractionResultsQueue = "document-extraction-results";
    public const string DocumentValidationQueue = "document-validation-queue";
    public const string DocumentValidationResultsQueue = "document-validation-results";
    public const string DocumentHumanReviewQueue = "document-review-queue";
    
}

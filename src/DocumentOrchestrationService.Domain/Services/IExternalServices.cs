namespace DocumentOrchestrationService.Domain.Services;

public interface IDocumentClassificationService
{
    Task<string> ClassifyDocumentAsync(string documentId, string blobUrl, string tenantId);
}

public interface IDocumentExtractionService
{
    Task<string> ExtractDataAsync(string documentId, string blobUrl, string tenantId, string documentType);
}

public interface IDataValidationService
{
    Task<(string validationResult, bool requiresHumanReview)> ValidateDataAsync(string extractedData, string documentType);
}

public interface IHumanReviewService
{
    Task<string> CreateReviewTaskAsync(string documentId, string extractedData, string validationResult);
    Task<bool> IsReviewCompleteAsync(string taskId);
    Task<string> GetReviewResultAsync(string taskId);
}

public interface IProcessedDataService
{
    Task<string> StoreAndIndexDataAsync(string documentId, string tenantId, string finalData);
}

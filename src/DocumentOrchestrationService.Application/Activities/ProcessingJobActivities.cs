// using Microsoft.Azure.Functions.Worker;
// using Microsoft.DurableTask;
// using Microsoft.Extensions.Logging;
// using DocumentOrchestrationService.Domain.Entities;
// using DocumentOrchestrationService.Domain.ValueObjects;
// using DocumentOrchestrationService.Domain.Repositories;
// using DocumentOrchestrationService.Domain.Services;

// namespace DocumentOrchestrationService.Application.Activities;

// public class ProcessingJobActivities
// {
//     private readonly IProcessingJobRepository _repository;
//     private readonly IDocumentClassificationService _classificationService;
//     private readonly IDocumentExtractionService _extractionService;
//     private readonly IDataValidationService _validationService;
//     private readonly IHumanReviewService _humanReviewService;
//     private readonly IProcessedDataService _processedDataService;
//     private readonly ILogger<ProcessingJobActivities> _logger;

//     public ProcessingJobActivities(
//         IProcessingJobRepository repository,
//         IDocumentClassificationService classificationService,
//         IDocumentExtractionService extractionService,
//         IDataValidationService validationService,
//         IHumanReviewService humanReviewService,
//         IProcessedDataService processedDataService,
//         ILogger<ProcessingJobActivities> logger)
//     {
//         _repository = repository;
//         _classificationService = classificationService;
//         _extractionService = extractionService;
//         _validationService = validationService;
//         _humanReviewService = humanReviewService;
//         _processedDataService = processedDataService;
//         _logger = logger;
//     }

    // [Function("CreateProcessingJob")]
    // public async Task<ProcessingJob> CreateProcessingJob([ActivityTrigger] DocumentIngestedMessage input)
    // {
    //     _logger.LogInformation("Creating processing job for document {DocumentId} from {SourceSystem}", 
    //         input.DocumentId, input.SourceSystem);

    //     var job = new ProcessingJob
    //     {
    //         Id = input.DocumentId.ToString(),
    //         DocumentId = input.DocumentId.ToString(),
    //         TenantId = input.TenantId,
    //         BlobUrl = input.BlobUrl,
    //         DocumentType = input.DocumentType,
    //         SourceSystem = input.SourceSystem,
    //         UserId = input.UserId,
    //         ClientReferenceId = input.ClientReferenceId,
    //         OverallStatus = ProcessingStatus.Processing
    //     };

    //     _logger.LogWarning("Creating processing job with ID {id}", job.Id);

    //     var createdJob = await _repository.CreateAsync(job);
    //     _logger.LogInformation("Created processing job {JobId} for document {DocumentId}", 
    //         createdJob.Id, input.DocumentId);
        
    //     return createdJob;
    // }

    // [Function("ClassifyDocument")]
    // public async Task<string> ClassifyDocument([ActivityTrigger] (string jobId, string tenantId) input)
    // {
    //     _logger.LogInformation("Starting classification for job {JobId}", input.jobId);
        
    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) 
    //     {
    //         _logger.LogError("Job {JobId} not found during classification", input.jobId);
    //         throw new InvalidOperationException($"Job {input.jobId} not found");
    //     }

    //     try
    //     {
    //         var result = await _classificationService.ClassifyDocumentAsync(job.DocumentId, job.BlobUrl, job.TenantId);
    //         _logger.LogInformation("Document {DocumentId} classified as {DocumentType}", 
    //             job.DocumentId, result);
    //         return result;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to classify document {DocumentId} for job {JobId}", 
    //             job.DocumentId, input.jobId);
    //         throw;
    //     }
    // }

    // [Function("UpdateJobClassification")]
    // public async Task UpdateJobClassification([ActivityTrigger] (string jobId, string tenantId, string classificationResult) input)
    // {
    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) return;

    //     job.ClassificationResult = input.classificationResult;
    //     job.OverallStatus = ProcessingStatus.Classified;
    //     await _repository.UpdateAsync(job);
    // }

    // [Function("ExtractData")]
    // public async Task<string> ExtractData([ActivityTrigger] (string jobId, string tenantId, string documentType) input)
    // {
    //     _logger.LogInformation("Starting data extraction for job {JobId} with document type {DocumentType}", 
    //         input.jobId, input.documentType);

    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) 
    //     {
    //         _logger.LogError("Job {JobId} not found during data extraction", input.jobId);
    //         throw new InvalidOperationException($"Job {input.jobId} not found");
    //     }

    //     try
    //     {
    //         var result = await _extractionService.ExtractDataAsync(job.DocumentId, job.BlobUrl, job.TenantId, input.documentType);
    //         _logger.LogInformation("Successfully extracted data for document {DocumentId}, size: {DataSize} characters", 
    //             job.DocumentId, result.Length);
    //         return result;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to extract data from document {DocumentId} for job {JobId}", 
    //             job.DocumentId, input.jobId);
    //         throw;
    //     }
    // }

    // [Function("UpdateJobExtraction")]
    // public async Task UpdateJobExtraction([ActivityTrigger] (string jobId, string tenantId, string extractionResult) input)
    // {
    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) return;

    //     job.ExtractionResult = input.extractionResult;
    //     job.OverallStatus = ProcessingStatus.Extracted;
    //     await _repository.UpdateAsync(job);
    // }

    // [Function("ValidateData")]
    // public async Task<(string result, bool requiresReview)> ValidateData([ActivityTrigger] (string extractionResult, string documentType) input)
    // {
    //     return await _validationService.ValidateDataAsync(input.extractionResult, input.documentType);
    // }

    // [Function("UpdateJobValidation")]
    // public async Task UpdateJobValidation([ActivityTrigger] (string jobId, string tenantId, string validationResult, bool requiresReview) input)
    // {
    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) return;

    //     job.ValidationResult = input.validationResult;
    //     job.RequiresHumanReview = input.requiresReview;
    //     job.OverallStatus = input.requiresReview ? ProcessingStatus.PendingHumanReview : ProcessingStatus.Validated;
    //     await _repository.UpdateAsync(job);
    // }

    // [Function("CreateReviewTask")]
    // public async Task<string> CreateReviewTask([ActivityTrigger] (string jobId, string extractedData, string validationResult) input)
    // {
    //     return await _humanReviewService.CreateReviewTaskAsync(input.jobId, input.extractedData, input.validationResult);
    // }

    // [Function("UpdateJobReviewTask")]
    // public async Task UpdateJobReviewTask([ActivityTrigger] (string jobId, string tenantId, string reviewTaskId) input)
    // {
    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) return;

    //     job.HumanReviewTaskId = input.reviewTaskId;
    //     await _repository.UpdateAsync(job);
    // }

    // [Function("CheckReviewStatus")]
    // public async Task<bool> CheckReviewStatus([ActivityTrigger] string taskId)
    // {
    //     return await _humanReviewService.IsReviewCompleteAsync(taskId);
    // }

    // [Function("GetReviewResult")]
    // public async Task<string> GetReviewResult([ActivityTrigger] string taskId)
    // {
    //     return await _humanReviewService.GetReviewResultAsync(taskId);
    // }

    // [Function("UpdateJobReviewed")]
    // public async Task UpdateJobReviewed([ActivityTrigger] (string jobId, string tenantId) input)
    // {
    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) return;

    //     job.OverallStatus = ProcessingStatus.Reviewed;
    //     await _repository.UpdateAsync(job);
    // }

    // [Function("StoreProcessedData")]
    // public async Task<string> StoreProcessedData([ActivityTrigger] (string jobId, string tenantId, string finalData) input)
    // {
    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) throw new InvalidOperationException($"Job {input.jobId} not found");

    //     return await _processedDataService.StoreAndIndexDataAsync(job.DocumentId, input.tenantId, input.finalData);
    // }

    // [Function("CompleteJob")]
    // public async Task CompleteJob([ActivityTrigger] (string jobId, string tenantId, string processedDataId) input)
    // {
    //     var job = await _repository.GetByIdAndTenantIdAsync(input.jobId, input.tenantId);
    //     if (job == null) return;

    //     job.ProcessedDataId = input.processedDataId;
    //     job.OverallStatus = ProcessingStatus.Completed;
    //     await _repository.UpdateAsync(job);
    // }

    // [Function("FailJob")]
    // public async Task FailJob([ActivityTrigger] (string documentId, string errorMessage) input)
    // {
    //     var job = await _repository.GetByDocumentIdAsync(input.documentId);
    //     if (job == null) return;

    //     job.ErrorMessage = input.errorMessage;
    //     job.OverallStatus = ProcessingStatus.Failed;
    //     await _repository.UpdateAsync(job);
    // }
// }

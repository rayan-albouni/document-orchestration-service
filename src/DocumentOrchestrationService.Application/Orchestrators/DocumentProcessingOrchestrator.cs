using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using DocumentOrchestrationService.Domain.Entities;
using DocumentOrchestrationService.Domain.ValueObjects;
using DocumentOrchestrationService.Domain.Repositories;
using DocumentOrchestrationService.Domain.Services;

namespace DocumentOrchestrationService.Application.Orchestrators;

public class DocumentProcessingOrchestrator
{
    private readonly IProcessingJobRepository _repository;
    private readonly IDocumentClassificationService _classificationService;
    private readonly IDocumentExtractionService _extractionService;
    private readonly IDataValidationService _validationService;
    private readonly IHumanReviewService _humanReviewService;
    private readonly IProcessedDataService _processedDataService;

    public DocumentProcessingOrchestrator(
        IProcessingJobRepository repository,
        IDocumentClassificationService classificationService,
        IDocumentExtractionService extractionService,
        IDataValidationService validationService,
        IHumanReviewService humanReviewService,
        IProcessedDataService processedDataService)
    {
        _repository = repository;
        _classificationService = classificationService;
        _extractionService = extractionService;
        _validationService = validationService;
        _humanReviewService = humanReviewService;
        _processedDataService = processedDataService;
    }

    [Function("DocumentProcessingOrchestrator")]
    public async Task<string> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<DocumentMessage>();
        if (input == null) return "Invalid input";

        try
        {
            var job = await context.CallActivityAsync<ProcessingJob>("CreateProcessingJob", input);

            var classificationResult = await context.CallActivityAsync<string>("ClassifyDocument", job.Id);
            await context.CallActivityAsync("UpdateJobClassification", (jobId: job.Id, classificationResult: classificationResult));

            var extractionResult = await context.CallActivityAsync<string>("ExtractData", (jobId: job.Id, documentType: classificationResult));
            await context.CallActivityAsync("UpdateJobExtraction", (jobId: job.Id, extractionResult: extractionResult));

            var validationResult = await context.CallActivityAsync<(string result, bool requiresReview)>("ValidateData", (extractionResult: extractionResult, documentType: classificationResult));
            await context.CallActivityAsync("UpdateJobValidation", (jobId: job.Id, validationResult: validationResult.result, requiresReview: validationResult.requiresReview));

            if (validationResult.requiresReview)
            {
                var reviewTaskId = await context.CallActivityAsync<string>("CreateReviewTask", (jobId: job.Id, extractedData: extractionResult, validationResult: validationResult.result));
                await context.CallActivityAsync("UpdateJobReviewTask", (jobId: job.Id, reviewTaskId: reviewTaskId));

                await context.CreateTimer(DateTime.UtcNow.AddMinutes(1), CancellationToken.None);
                var isReviewComplete = await context.CallActivityAsync<bool>("CheckReviewStatus", reviewTaskId);
                
                while (!isReviewComplete)
                {
                    await context.CreateTimer(DateTime.UtcNow.AddMinutes(5), CancellationToken.None);
                    isReviewComplete = await context.CallActivityAsync<bool>("CheckReviewStatus", reviewTaskId);
                }

                extractionResult = await context.CallActivityAsync<string>("GetReviewResult", reviewTaskId);
                await context.CallActivityAsync("UpdateJobReviewed", job.Id);
            }

            var processedDataId = await context.CallActivityAsync<string>("StoreProcessedData", (jobId: job.Id, tenantId: input.TenantId, finalData: extractionResult));
            await context.CallActivityAsync("CompleteJob", (jobId: job.Id, processedDataId: processedDataId));

            return "Processing completed successfully";
        }
        catch (Exception ex)
        {
            await context.CallActivityAsync("FailJob", (documentId: input.DocumentId, errorMessage: ex.Message));
            return $"Processing failed: {ex.Message}";
        }
    }
}

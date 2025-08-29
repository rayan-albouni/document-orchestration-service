using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using DocumentOrchestrationService.Domain.Entities;
using DocumentOrchestrationService.Domain.ValueObjects;

namespace DocumentOrchestrationService.Application.Orchestrators;

public class DocumentProcessingOrchestratorAsync
{
    [Function("DocumentProcessingOrchestratorAsync")]
    public async Task<string> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<DocumentProcessingOrchestratorAsync>();
        var input = context.GetInput<DocumentIngestedMessage>();
        if (input == null) 
        {
            logger.LogWarning("Orchestrator received null input");
            return "Invalid input";
        }

        logger.LogInformation("Starting async orchestration for document {DocumentId}", input.DocumentId);

        try
        {
            // Step 1: Create processing job 
            var job = await context.CallActivityAsync<ProcessingJob>("CreateProcessingJobAsync", input);
            logger.LogInformation("Created processing job {JobId} for document {DocumentId}", job.Id, input.DocumentId);

            // Step 2: Send document to classification queue using activity function
            var message = new DocumentToClassifyMessage
            {
                DocumentId = input.DocumentId,
                TenantId = input.TenantId,
                BlobUrl = input.BlobUrl
            };
            await context.CallActivityAsync("SendDocumentToClassificationQueue", message);
            logger.LogInformation("Sent document {DocumentId} to classification queue", input.DocumentId);

            // At this point, the orchestration completes. The classification and extraction
            // will be handled by separate Service Bus triggered functions that update the job status.
            logger.LogInformation("Async orchestration initiated for document {DocumentId}", input.DocumentId);
            return "Async processing initiated successfully";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initiate async processing for document {DocumentId}", input.DocumentId);
            await context.CallActivityAsync("FailJobAsync", (documentId: input.DocumentId.ToString(), tenantId: input.TenantId.ToString(), errorMessage: ex.Message));
            return $"Processing failed: {ex.Message}";
        }
    }
}

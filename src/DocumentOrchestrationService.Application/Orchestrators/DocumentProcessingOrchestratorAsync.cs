using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using DocumentOrchestrationService.Domain.Entities;
using DocumentOrchestrationService.Domain.ValueObjects;
using DocumentOrchestrationService.Domain.Services;
using DocumentOrchestrationService.Domain.Constants;

namespace DocumentOrchestrationService.Application.Orchestrators;

public class DocumentProcessingOrchestratorAsync
{
    private readonly IMessagingBusService _messagingBusService;

    public DocumentProcessingOrchestratorAsync(IMessagingBusService messagingBusService)
    {
        _messagingBusService = messagingBusService;
    }

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

            // Step 2: Send document to classification queue
            var message = new DocumentToClassifyMessage
            {
                DocumentId = input.DocumentId,
                TenantId = input.TenantId,
                BlobUrl = input.BlobUrl
            };
            await _messagingBusService.SendMessageAsync(ServiceBusQueues.DocumentClassificationQueue, message);
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

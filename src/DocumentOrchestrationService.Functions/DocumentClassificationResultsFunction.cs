using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DocumentOrchestrationService.Domain.ValueObjects;
using DocumentOrchestrationService.Domain.Services;
using DocumentOrchestrationService.Domain.Constants;

namespace DocumentOrchestrationService.Functions;

public class DocumentClassificationResultsFunction
{
    private readonly ILogger<DocumentClassificationResultsFunction> _logger;
    private readonly IServiceBusService _serviceBusService;

    public DocumentClassificationResultsFunction(
        ILogger<DocumentClassificationResultsFunction> logger, 
        IServiceBusService serviceBusService)
    {
        _logger = logger;
        _serviceBusService = serviceBusService;
    }

    [Function("DocumentClassificationResultsFunction")]
    public async Task Run(
        [ServiceBusTrigger(ServiceBusQueues.DocumentClassificationResultsQueue, Connection = "ServiceBusConnectionString")] string message,
        [DurableClient] DurableTaskClient client)
    {
        _logger.LogInformation("Document classification results received: {MessageLength} characters", message.Length);

        try
        {
            var classifiedMessage = JsonConvert.DeserializeObject<DocumentClassifiedMessage>(message);
            if (classifiedMessage == null)
            {
                _logger.LogWarning("Failed to deserialize classification results message. Message: {Message}", message);
                return;
            }

            _logger.LogInformation("Processing classification result for document {DocumentId} classified as {DocumentType} with confidence {ConfidenceScore}", 
                classifiedMessage.DocumentId, classifiedMessage.DocumentType, classifiedMessage.ConfidenceScore);

            // Update the processing job with classification results
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "UpdateClassificationOrchestrator",
                classifiedMessage);

            _logger.LogInformation("Started classification update orchestration {InstanceId} for document {DocumentId}", 
                instanceId, classifiedMessage.DocumentId);

            // Send document to extraction queue
            var extractionMessage = new DocumentToExtractMessage
            {
                DocumentId = classifiedMessage.DocumentId,
                TenantId = classifiedMessage.TenantId,
                DocumentType = classifiedMessage.DocumentType,
                BlobUrl = classifiedMessage.BlobUrl
            };

            await _serviceBusService.SendMessageAsync(ServiceBusQueues.DocumentExtractionQueue, extractionMessage);
            _logger.LogInformation("Sent document {DocumentId} to extraction queue for type {DocumentType}", 
                classifiedMessage.DocumentId, classifiedMessage.DocumentType);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse classification results JSON message: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing classification results");
        }
    }
}

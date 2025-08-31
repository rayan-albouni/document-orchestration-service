using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DocumentOrchestrationService.Domain.ValueObjects;
using DocumentOrchestrationService.Domain.Constants;

namespace DocumentOrchestrationService.Functions;

public class DocumentExtractionResultsFunction
{
    private readonly ILogger<DocumentExtractionResultsFunction> _logger;

    public DocumentExtractionResultsFunction(ILogger<DocumentExtractionResultsFunction> logger)
    {
        _logger = logger;
    }

    [Function("DocumentExtractionResultsFunction")]
    public async Task Run(
        [ServiceBusTrigger(ServiceBusQueues.DocumentExtractionResultsQueue, Connection = "ServiceBusConnectionString")] string message,
        [DurableClient] DurableTaskClient client)
    {
        _logger.LogInformation("Document extraction results received: {MessageLength} characters", message.Length);

        try
        {
            // Deserialize directly using DocumentExtractedMessage with custom converter
            var extractedMessage = JsonConvert.DeserializeObject<DocumentExtractedMessage>(message);
            if (extractedMessage == null)
            {
                _logger.LogWarning("Failed to deserialize extraction results message. Message: {Message}", message);
                return;
            }

            _logger.LogInformation("Processing extraction result for document {DocumentId}", extractedMessage.DocumentId);

            // Update the processing job with extraction results
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "UpdateExtractionOrchestrator",
                extractedMessage);

            _logger.LogInformation("Started extraction update orchestration {InstanceId} for document {DocumentId}", 
                instanceId, extractedMessage.DocumentId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse extraction results JSON message: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing extraction results");
        }
    }
}

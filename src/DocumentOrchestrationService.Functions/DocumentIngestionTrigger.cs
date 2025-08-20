using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DocumentOrchestrationService.Functions.Models;

namespace DocumentOrchestrationService.Functions;

public class DocumentIngestionTrigger
{
    private readonly ILogger<DocumentIngestionTrigger> _logger;

    public DocumentIngestionTrigger(ILogger<DocumentIngestionTrigger> logger)
    {
        _logger = logger;
    }

    [Function("DocumentIngestionTrigger")]
    public async Task Run(
        [ServiceBusTrigger("document-ingestion-queue", Connection = "ServiceBusConnectionString")] string message,
        [DurableClient] DurableTaskClient client)
    {
        _logger.LogInformation("Document ingestion triggered with message: {MessageLength} characters", message.Length);

        try
        {
            var documentMessageDto = JsonConvert.DeserializeObject<DocumentMessageDto>(message);
            if (documentMessageDto == null)
            {
                _logger.LogWarning("Failed to deserialize document message. Message: {Message}", message);
                return;
            }

            var documentMessage = documentMessageDto.ToDomainObject();
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "DocumentProcessingOrchestrator",
                documentMessage);

            _logger.LogInformation("Started orchestration {InstanceId} for document {DocumentId}", 
                instanceId, documentMessage.DocumentId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON message: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing document ingestion");
        }
    }
}

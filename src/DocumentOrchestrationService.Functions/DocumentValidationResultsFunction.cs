using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DocumentOrchestrationService.Domain.ValueObjects;
using DocumentOrchestrationService.Domain.Constants;
using DocumentOrchestrationService.Domain.Services;

namespace DocumentOrchestrationService.Functions;

public class DocumentValidationResultsFunction
{

    private readonly IMessagingBusService _messagingBusService;
    private readonly ILogger<DocumentValidationResultsFunction> _logger;

    public DocumentValidationResultsFunction(IMessagingBusService messagingBusService, ILogger<DocumentValidationResultsFunction> logger)
    {
        _messagingBusService = messagingBusService;
        _logger = logger;
    }

    [Function("DocumentValidationResultsFunction")]
    public async Task Run(
        [ServiceBusTrigger(ServiceBusQueues.DocumentValidationResultsQueue, Connection = "ServiceBusConnectionString")] string message,
        [DurableClient] DurableTaskClient client)
    {
        _logger.LogInformation("Document validation results received: {MessageLength} characters", message.Length);

        try
        {
            var validatedMessage = JsonConvert.DeserializeObject<DocumentValidatedMessage>(message);
            if (validatedMessage == null)
            {
                _logger.LogWarning("Failed to deserialize validation results message. Message: {Message}", message);
                return;
            }

            _logger.LogInformation("Processing validation result for document {DocumentId}", validatedMessage.DocumentId);

            // Update the processing job with validation results
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "UpdateValidationOrchestrator",
                validatedMessage);

            _logger.LogInformation("Started validation update orchestration {InstanceId} for document {DocumentId}",
                instanceId, validatedMessage.DocumentId);

            if (!validatedMessage.IsValid)
            {
            await _messagingBusService.SendMessageAsync(ServiceBusQueues.DocumentHumanReviewQueue, validatedMessage);
            _logger.LogInformation("Sent document {DocumentId} to human review queue",
                validatedMessage.DocumentId);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse validation results JSON message: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing validation results");
        }
    }
}

using DocumentOrchestrationService.Domain.Constants;
using DocumentOrchestrationService.Domain.Services;
using DocumentOrchestrationService.Domain.ValueObjects;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocumentOrchestrationService.Application.Activities;

public class ServiceBusActivities
{
    private readonly IMessagingBusService _serviceBusService;
    private readonly ILogger<ServiceBusActivities> _logger;

    public ServiceBusActivities(IMessagingBusService serviceBusService, ILogger<ServiceBusActivities> logger)
    {
        _serviceBusService = serviceBusService;
        _logger = logger;
    }

    [Function("SendDocumentToClassificationQueue")]
    public async Task SendDocumentToClassificationQueue([ActivityTrigger] DocumentToClassifyMessage message)
    {
        _logger.LogInformation("Sending document {DocumentId} to classification queue", message.DocumentId);

        try
        {
            await _serviceBusService.SendMessageAsync(ServiceBusQueues.DocumentClassificationQueue, message);
            _logger.LogInformation("Successfully sent document {DocumentId} to classification queue", message.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send document {DocumentId} to classification queue", message.DocumentId);
            throw;
        }
    }

    [Function("SendDocumentToExtractionQueue")]
    public async Task SendDocumentToExtractionQueue([ActivityTrigger] DocumentToExtractMessage message)
    {
        _logger.LogInformation("Sending document {DocumentId} to extraction queue", message.DocumentId);

        try
        {
            await _serviceBusService.SendMessageAsync(ServiceBusQueues.DocumentExtractionQueue, message);
            _logger.LogInformation("Successfully sent document {DocumentId} to extraction queue", message.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send document {DocumentId} to extraction queue", message.DocumentId);
            throw;
        }
    }

    [Function("SendDocumentToValidationQueue")]
    public async Task SendDocumentToValidationQueue([ActivityTrigger] DocumentToValidateMessage message)
    {
        _logger.LogInformation("Sending document {DocumentId} to validation queue", message.DocumentId);

        try
        {
            await _serviceBusService.SendMessageAsync(ServiceBusQueues.DocumentValidationQueue, message);
            _logger.LogInformation("Successfully sent document {DocumentId} to validation queue", message.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send document {DocumentId} to validation queue", message.DocumentId);
            throw;
        }
    }
}

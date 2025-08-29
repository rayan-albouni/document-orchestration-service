using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DocumentOrchestrationService.Domain.Services;

namespace DocumentOrchestrationService.Infrastructure.Services;

public class MessagingBusService : IMessagingBusService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<MessagingBusService> _logger;

    public MessagingBusService(ServiceBusClient serviceBusClient, ILogger<MessagingBusService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    public async Task SendMessageAsync<T>(string queueName, T message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
        }

        try
        {
            _logger.LogDebug("Sending message to queue {QueueName} for message type {MessageType}", 
                queueName, typeof(T).Name);

            // Serialize the message to JSON
            var messageBody = JsonConvert.SerializeObject(message);
            
            // Create a Service Bus message
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString()
            };

            // Add custom properties for better tracking
            serviceBusMessage.ApplicationProperties.Add("MessageType", typeof(T).Name);
            serviceBusMessage.ApplicationProperties.Add("Timestamp", DateTimeOffset.UtcNow);

            // Send the message
            await using var sender = _serviceBusClient.CreateSender(queueName);
            await sender.SendMessageAsync(serviceBusMessage);

            _logger.LogInformation("Successfully sent message {MessageId} to queue {QueueName}", 
                serviceBusMessage.MessageId, queueName);
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex, "Service Bus error occurred while sending message to queue {QueueName}: {ErrorReason}", 
                queueName, ex.Reason);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization error occurred while sending message to queue {QueueName}", 
                queueName);
            throw new InvalidOperationException($"Failed to serialize message for queue {queueName}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while sending message to queue {QueueName}", 
                queueName);
            throw;
        }
    }
}

using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Newtonsoft.Json;
using DocumentOrchestrationService.Domain.ValueObjects;

namespace DocumentOrchestrationService.Functions;

public class DocumentIngestionTrigger
{
    [Function("DocumentIngestionTrigger")]
    public async Task Run(
        [ServiceBusTrigger("document-ingestion-queue", Connection = "ServiceBusConnectionString")] string message,
        [DurableClient] DurableTaskClient client)
    {
        var documentMessage = JsonConvert.DeserializeObject<DocumentMessage>(message);
        if (documentMessage == null) return;

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            "DocumentProcessingOrchestrator",
            documentMessage);
    }
}

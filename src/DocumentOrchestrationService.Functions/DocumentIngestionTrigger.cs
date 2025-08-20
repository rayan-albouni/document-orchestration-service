using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Newtonsoft.Json;
using DocumentOrchestrationService.Functions.Models;

namespace DocumentOrchestrationService.Functions;

public class DocumentIngestionTrigger
{
    [Function("DocumentIngestionTrigger")]
    public async Task Run(
        [ServiceBusTrigger("document-ingestion-queue", Connection = "ServiceBusConnectionString")] string message,
        [DurableClient] DurableTaskClient client)
    {
        var documentMessageDto = JsonConvert.DeserializeObject<DocumentMessageDto>(message);
        if (documentMessageDto == null) return;

        var documentMessage = documentMessageDto.ToDomainObject();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            "DocumentProcessingOrchestrator",
            documentMessage);
    }
}

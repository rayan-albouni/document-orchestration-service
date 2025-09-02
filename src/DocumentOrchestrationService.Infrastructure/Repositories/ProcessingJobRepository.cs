using DocumentOrchestrationService.Domain.Entities;
using DocumentOrchestrationService.Domain.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DocumentOrchestrationService.Infrastructure.Repositories;

public class ProcessingJobRepository : IProcessingJobRepository
{
    private readonly Container _container;
    private readonly ILogger<ProcessingJobRepository> _logger;

    public ProcessingJobRepository(CosmosClient cosmosClient, string databaseId, string containerId, ILogger<ProcessingJobRepository> logger)
    {
        _container = cosmosClient.GetContainer(databaseId, containerId);
        _logger = logger;
    }
    public async Task<ProcessingJob?> GetByIdAndTenantIdAsync(string id, string tenantId)
    {
        try
        {
            var response = await _container.ReadItemAsync<ProcessingJob>(id, new PartitionKey(tenantId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<ProcessingJob?> GetByDocumentIdAsync(string id)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        var iterator = _container.GetItemQueryIterator<ProcessingJob>(query);
        var results = await iterator.ReadNextAsync();
        return results.FirstOrDefault();
    }

    public async Task<ProcessingJob> CreateAsync(ProcessingJob job)
    {
        _logger.LogInformation("Creating processing job {JobId} for document {DocumentId}", job.Id, job.DocumentId);

        var response = await _container.CreateItemAsync(job, new PartitionKey(job.TenantId));

        _logger.LogInformation("Successfully created processing job {JobId} for document {DocumentId}",
            job.Id, job.DocumentId);

        return response.Resource;
    }

    public async Task<ProcessingJob> UpdateAsync(ProcessingJob job)
    {
        _logger.LogDebug("Updating processing job {JobId} with status {Status}", job.Id, job.OverallStatus);

        job.UpdatedAt = DateTime.UtcNow;
        var response = await _container.ReplaceItemAsync(job, job.Id, new PartitionKey(job.TenantId));
        return response.Resource;
    }
}

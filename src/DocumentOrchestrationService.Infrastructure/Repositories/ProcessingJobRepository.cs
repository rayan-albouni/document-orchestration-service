using Microsoft.Azure.Cosmos;
using DocumentOrchestrationService.Domain.Entities;
using DocumentOrchestrationService.Domain.Repositories;

namespace DocumentOrchestrationService.Infrastructure.Repositories;

public class ProcessingJobRepository : IProcessingJobRepository
{
    private readonly Container _container;

    public ProcessingJobRepository(CosmosClient cosmosClient, string databaseName)
    {
        _container = cosmosClient.GetContainer(databaseName, "ProcessingJobs");
    }

    public async Task<ProcessingJob?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<ProcessingJob>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<ProcessingJob?> GetByDocumentIdAsync(string documentId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.DocumentId = @documentId")
            .WithParameter("@documentId", documentId);

        var iterator = _container.GetItemQueryIterator<ProcessingJob>(query);
        var results = await iterator.ReadNextAsync();
        return results.FirstOrDefault();
    }

    public async Task<ProcessingJob> CreateAsync(ProcessingJob job)
    {
        var response = await _container.CreateItemAsync(job, new PartitionKey(job.Id));
        return response.Resource;
    }

    public async Task<ProcessingJob> UpdateAsync(ProcessingJob job)
    {
        job.UpdatedAt = DateTime.UtcNow;
        var response = await _container.ReplaceItemAsync(job, job.Id, new PartitionKey(job.Id));
        return response.Resource;
    }
}

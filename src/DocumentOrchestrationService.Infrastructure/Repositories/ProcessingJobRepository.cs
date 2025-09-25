using DocumentOrchestrationService.Domain.Entities;
using DocumentOrchestrationService.Domain.Repositories;
using DocumentOrchestrationService.Domain.ValueObjects;
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

    public async Task<PaginatedDocumentsResponse> GetDocumentsByTenantAsync(DocumentsQueryRequest request)
    {
        _logger.LogInformation("Retrieving documents for tenant {TenantId} with page size {PageSize}",
            request.TenantId, request.PageSize);

        try
        {
            // Simple partition-scoped query with sorting for optimal performance
            // Since tenantId is the partition key, we only need to scan within the partition
            var sortField = request.SortBy?.ToLowerInvariant() switch
            {
                "createdat" => "c.createdAt",
                "updatedat" => "c.updatedAt",
                "documenttype" => "c.documentType",
                "status" => "c.overallStatus",
                _ => "c.createdAt"
            };

            var sortDirection = request.SortOrder?.ToLowerInvariant() == "asc" ? "ASC" : "DESC";
            var queryText = $"SELECT * FROM c ORDER BY {sortField} {sortDirection}";

            var queryDefinition = new QueryDefinition(queryText);

            // Create query request options with partition key for single-partition query
            var requestOptions = new QueryRequestOptions
            {
                MaxItemCount = request.PageSize,
                PartitionKey = new PartitionKey(request.TenantId) // This ensures we only query the specific partition
            };

            var iterator = _container.GetItemQueryIterator<ProcessingJob>(
                queryDefinition,
                request.ContinuationToken,
                requestOptions);

            var documents = new List<DocumentSummary>();
            string? continuationToken = null;
            bool hasMoreResults = false;

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();

                foreach (var job in response)
                {
                    documents.Add(new DocumentSummary(
                        job.DocumentId,
                        job.TenantId,
                        job.DocumentType,
                        job.SourceSystem,
                        job.UserId,
                        job.ClientReferenceId,
                        job.OverallStatus,
                        job.CreatedAt,
                        job.UpdatedAt,
                        job.RequiresHumanReview,
                        job.ErrorMessage,
                        job.BlobUrl,
                        job.ClassificationResult
                    ));
                }

                continuationToken = response.ContinuationToken;
                hasMoreResults = !string.IsNullOrEmpty(continuationToken);
            }

            _logger.LogInformation("Retrieved {DocumentCount} documents for tenant {TenantId}",
                documents.Count, request.TenantId);

            return new PaginatedDocumentsResponse(
                documents,
                documents.Count, // Page count, not total count
                request.PageSize,
                continuationToken,
                hasMoreResults
            );
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error occurred while retrieving documents for tenant {TenantId}",
                request.TenantId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving documents for tenant {TenantId}",
                request.TenantId);
            throw;
        }
    }
}

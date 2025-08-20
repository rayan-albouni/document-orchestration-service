using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using DocumentOrchestrationService.Domain.Repositories;
using DocumentOrchestrationService.Domain.ValueObjects;

namespace DocumentOrchestrationService.Functions;

public class DocumentStatusFunction
{
    private readonly IProcessingJobRepository _repository;
    private readonly ILogger<DocumentStatusFunction> _logger;

    public DocumentStatusFunction(IProcessingJobRepository repository, ILogger<DocumentStatusFunction> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [Function("GetDocumentStatus")]
    public async Task<HttpResponseData> GetDocumentStatus(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/v1/documents/{documentId}/status")] HttpRequestData req,
        string documentId)
    {
        _logger.LogInformation("Status requested for document {DocumentId}", documentId);

        try
        {
            var job = await _repository.GetByDocumentIdAsync(documentId);
            if (job == null)
            {
                _logger.LogWarning("Document {DocumentId} not found", documentId);
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync(JsonConvert.SerializeObject(new { error = "Document not found" }));
                return notFoundResponse;
            }

            _logger.LogInformation("Found document {DocumentId} with status {Status} for tenant {TenantId}", 
                documentId, job.OverallStatus, job.TenantId);

            var statusResponse = new DocumentStatusResponse(
                job.DocumentId,
                job.TenantId,
                job.OverallStatus.ToString(),
                job.CreatedAt,
                job.UpdatedAt,
                job.RequiresHumanReview,
                job.HumanReviewTaskId,
                job.ErrorMessage
            );

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonConvert.SerializeObject(statusResponse));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving status for document {DocumentId}", documentId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync(JsonConvert.SerializeObject(new { error = ex.Message }));
            return errorResponse;
        }
    }
}

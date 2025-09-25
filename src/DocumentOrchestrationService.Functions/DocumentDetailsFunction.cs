using System.Net;
using DocumentOrchestrationService.Domain.Repositories;
using DocumentOrchestrationService.Domain.ValueObjects;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DocumentOrchestrationService.Functions;

public class DocumentDetailsFunction
{
    private readonly IProcessingJobRepository _repository;
    private readonly ILogger<DocumentDetailsFunction> _logger;

    public DocumentDetailsFunction(IProcessingJobRepository repository, ILogger<DocumentDetailsFunction> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [Function("GetDocumentDetails")]
    public async Task<HttpResponseData> GetDocumentDetails(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/v1/documents/{documentId}/details")] HttpRequestData req,
        string documentId)
    {
        _logger.LogInformation("Details requested for document {DocumentId}", documentId);

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

            var detailsResponse = new DocumentDetailsResponse(
                job.DocumentId,
                job.TenantId,
                job.BlobUrl,
                job.DocumentType,
                job.SourceSystem,
                job.UserId,
                job.ClientReferenceId,
                job.OverallStatus.ToString(),
                job.CreatedAt,
                job.UpdatedAt,
                job.ClassificationResult,
                job.ClassificationConfidenceScore,
                job.ExtractionResult,
                job.ValidationResult,
                job.RequiresHumanReview,
                job.HumanReviewTaskId,
                job.ProcessedDataId,
                job.ErrorMessage
            );

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonConvert.SerializeObject(detailsResponse));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving details for document {DocumentId}", documentId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync(JsonConvert.SerializeObject(new { error = ex.Message }));
            return errorResponse;
        }
    }
}
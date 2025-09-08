using System.Net;
using DocumentOrchestrationService.Domain.Repositories;
using DocumentOrchestrationService.Domain.ValueObjects;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DocumentOrchestrationService.Functions;

public class DocumentsListFunction
{
  private readonly IProcessingJobRepository _repository;
  private readonly ILogger<DocumentsListFunction> _logger;

  public DocumentsListFunction(IProcessingJobRepository repository, ILogger<DocumentsListFunction> logger)
  {
    _repository = repository;
    _logger = logger;
  }

  [Function("GetDocumentsList")]
  public async Task<HttpResponseData> GetDocumentsList(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/v1/tenants/{tenantId}/documents")] HttpRequestData req,
      string tenantId)
  {
    _logger.LogInformation("Documents list requested for tenant {TenantId}", tenantId);

    try
    {
      // Parse query parameters
      var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

      var request = new DocumentsQueryRequest(
          TenantId: tenantId,
          PageSize: int.TryParse(query["pageSize"], out var pageSize) ? Math.Min(pageSize, 100) : 20, // Cap at 100
          ContinuationToken: query["continuationToken"],
          SortBy: query["sortBy"] ?? "CreatedAt",
          SortOrder: query["sortOrder"] ?? "desc"
      );

      _logger.LogInformation("Retrieving documents for tenant {TenantId} with PageSize={PageSize}, SortBy={SortBy}",
          tenantId, request.PageSize, request.SortBy);

      var result = await _repository.GetDocumentsByTenantAsync(request);

      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "application/json");
      await response.WriteStringAsync(JsonConvert.SerializeObject(result, new JsonSerializerSettings
      {
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
      }));

      _logger.LogInformation("Successfully retrieved {DocumentCount} documents for tenant {TenantId}, HasMoreResults={HasMoreResults}",
          result.Documents.Count, tenantId, result.HasMoreResults);

      return response;
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning(ex, "Invalid request parameters for tenant {TenantId}", tenantId);
      var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
      await badRequestResponse.WriteStringAsync(JsonConvert.SerializeObject(new { error = "Invalid request parameters", details = ex.Message }));
      return badRequestResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving documents list for tenant {TenantId}", tenantId);
      var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
      await errorResponse.WriteStringAsync(JsonConvert.SerializeObject(new { error = "Internal server error", details = ex.Message }));
      return errorResponse;
    }
  }
}

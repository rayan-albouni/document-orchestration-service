using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DocumentOrchestrationService.Domain.Services;

namespace DocumentOrchestrationService.Infrastructure.Services;

public class DocumentClassificationService : IDocumentClassificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _secret;
    private readonly ILogger<DocumentClassificationService> _logger;

    public DocumentClassificationService(HttpClient httpClient, IConfiguration configuration, ILogger<DocumentClassificationService> logger)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["DocumentClassificationServiceUrl"] ?? throw new InvalidOperationException("DocumentClassificationServiceUrl not configured");
        _secret = configuration["DocumentClassificationServiceSecret"] ?? throw new InvalidOperationException("DocumentClassificationServiceSecret not configured");
        _logger = logger;
    }

    public async Task<string> ClassifyDocumentAsync(string documentId, string blobUrl, string tenantId)
    {
        _logger.LogInformation("Classifying document {DocumentId} for tenant {TenantId} from blob {BlobUrl}", 
            documentId, tenantId, blobUrl);

        try
        {
            var request = new { DocumentId = documentId, BlobUrl = blobUrl, TenantId = tenantId };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/documents/{documentId}/classify?code={_secret}", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
            string documentType = result?.documentType?.ToString() ?? "unknown";
            
            _logger.LogInformation("Document {DocumentId} classified as type: {DocumentType}", documentId, documentType);
            return documentType;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error classifying document {DocumentId}", documentId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error classifying document {DocumentId}", documentId);
            throw;
        }
    }
}

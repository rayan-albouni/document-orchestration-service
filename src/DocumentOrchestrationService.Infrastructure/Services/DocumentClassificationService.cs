using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using DocumentOrchestrationService.Domain.Services;

namespace DocumentOrchestrationService.Infrastructure.Services;

public class DocumentClassificationService : IDocumentClassificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DocumentClassificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["DocumentClassificationServiceUrl"] ?? throw new InvalidOperationException("DocumentClassificationServiceUrl not configured");
    }

    public async Task<string> ClassifyDocumentAsync(string documentId, string blobUrl, string tenantId)
    {
        var request = new { DocumentId = documentId, BlobUrl = blobUrl, TenantId = tenantId };
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/classify", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
        return result?.documentType?.ToString() ?? "unknown";
    }
}

using System.Text;
using DocumentOrchestrationService.Domain.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DocumentOrchestrationService.Infrastructure.Services;

public class DocumentExtractionService : IDocumentExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DocumentExtractionService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["DocumentExtractionServiceUrl"] ?? throw new InvalidOperationException("DocumentExtractionServiceUrl not configured");
    }

    public async Task<string> ExtractDataAsync(string documentId, string blobUrl, string tenantId, string documentType)
    {
        var request = new { DocumentId = documentId, BlobUrl = blobUrl, TenantId = tenantId, DocumentType = documentType };
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/extract", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}

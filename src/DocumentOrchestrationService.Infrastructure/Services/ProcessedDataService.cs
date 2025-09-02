using System.Text;
using DocumentOrchestrationService.Domain.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DocumentOrchestrationService.Infrastructure.Services;

public class ProcessedDataService : IProcessedDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ProcessedDataService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ProcessedDataServiceUrl"] ?? throw new InvalidOperationException("ProcessedDataServiceUrl not configured");
    }

    public async Task<string> StoreAndIndexDataAsync(string documentId, string tenantId, string finalData)
    {
        var request = new { DocumentId = documentId, TenantId = tenantId, FinalData = finalData };
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/store", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
        return result?.processedDataId?.ToString() ?? string.Empty;
    }
}

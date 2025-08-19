using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using DocumentOrchestrationService.Domain.Services;

namespace DocumentOrchestrationService.Infrastructure.Services;

public class DataValidationService : IDataValidationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DataValidationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["DataValidationServiceUrl"] ?? throw new InvalidOperationException("DataValidationServiceUrl not configured");
    }

    public async Task<(string validationResult, bool requiresHumanReview)> ValidateDataAsync(string extractedData, string documentType)
    {
        var request = new { ExtractedData = extractedData, DocumentType = documentType };
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/validate", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
        
        return (
            result?.validationResult?.ToString() ?? string.Empty,
            result?.requiresHumanReview?.ToObject<bool>() ?? false
        );
    }
}

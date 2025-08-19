using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using DocumentOrchestrationService.Domain.Services;

namespace DocumentOrchestrationService.Infrastructure.Services;

public class HumanReviewService : IHumanReviewService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HumanReviewService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["HumanReviewServiceUrl"] ?? throw new InvalidOperationException("HumanReviewServiceUrl not configured");
    }

    public async Task<string> CreateReviewTaskAsync(string documentId, string extractedData, string validationResult)
    {
        var request = new { DocumentId = documentId, ExtractedData = extractedData, ValidationResult = validationResult };
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/review", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
        return result?.taskId?.ToString() ?? string.Empty;
    }

    public async Task<bool> IsReviewCompleteAsync(string taskId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/review/{taskId}/status");
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
        return result?.isComplete?.ToObject<bool>() ?? false;
    }

    public async Task<string> GetReviewResultAsync(string taskId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/review/{taskId}/result");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}

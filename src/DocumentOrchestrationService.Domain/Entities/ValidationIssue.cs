using DocumentOrchestrationService.Domain.Enums;
using Newtonsoft.Json;

namespace DocumentOrchestrationService.Domain.Entities;

public class ValidationIssue
{
  [JsonProperty("FieldName")]
  public string FieldName { get; set; } = string.Empty;

  [JsonProperty("IssueType")]
  public ValidationIssueType IssueType { get; set; }

  [JsonProperty("Description")]
  public string Description { get; set; } = string.Empty;

  [JsonProperty("Severity")]
  public ValidationSeverity Severity { get; set; }
}

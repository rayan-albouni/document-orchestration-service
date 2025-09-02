using DocumentOrchestrationService.Domain.Entities;

namespace DocumentOrchestrationService.Domain.ValueObjects;

public record DocumentValidatedMessage
{
    public Guid DocumentId { get; set; }
    public required string TenantId { get; set; }
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
}
using DocumentOrchestrationService.Domain.Entities;

namespace DocumentOrchestrationService.Domain.Repositories;

public interface IProcessingJobRepository
{
    
    Task<ProcessingJob?> GetByIdAndTenantIdAsync(string id, string tenantId);
    Task<ProcessingJob?> GetByDocumentIdAsync(string documentId);
    Task<ProcessingJob> CreateAsync(ProcessingJob job);
    Task<ProcessingJob> UpdateAsync(ProcessingJob job);
}

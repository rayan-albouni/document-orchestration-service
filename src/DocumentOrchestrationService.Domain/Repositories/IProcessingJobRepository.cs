using DocumentOrchestrationService.Domain.Entities;

namespace DocumentOrchestrationService.Domain.Repositories;

public interface IProcessingJobRepository
{
    Task<ProcessingJob?> GetByIdAsync(string id);
    Task<ProcessingJob?> GetByDocumentIdAsync(string documentId);
    Task<ProcessingJob> CreateAsync(ProcessingJob job);
    Task<ProcessingJob> UpdateAsync(ProcessingJob job);
}

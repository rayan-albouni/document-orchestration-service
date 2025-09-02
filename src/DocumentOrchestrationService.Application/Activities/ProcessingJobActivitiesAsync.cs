using DocumentOrchestrationService.Domain.Entities;
using DocumentOrchestrationService.Domain.Repositories;
using DocumentOrchestrationService.Domain.ValueObjects;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocumentOrchestrationService.Application.Activities;

public class ProcessingJobActivitiesAsync
{
  private readonly IProcessingJobRepository _repository;
  private readonly ILogger<ProcessingJobActivitiesAsync> _logger;

  public ProcessingJobActivitiesAsync(
      IProcessingJobRepository repository,
      ILogger<ProcessingJobActivitiesAsync> logger)
  {
    _repository = repository;
    _logger = logger;
  }

  [Function("CreateProcessingJobAsync")]
  public async Task<ProcessingJob> CreateProcessingJobAsync([ActivityTrigger] DocumentIngestedMessage input)
  {
    _logger.LogInformation("Creating processing job for document {DocumentId} from {SourceSystem}",
        input.DocumentId, input.SourceSystem);

    var job = new ProcessingJob
    {
      Id = input.DocumentId.ToString(),
      DocumentId = input.DocumentId.ToString(),
      TenantId = input.TenantId,
      BlobUrl = input.BlobUrl,
      DocumentType = input.DocumentType,
      SourceSystem = input.SourceSystem,
      UserId = input.UserId,
      ClientReferenceId = input.ClientReferenceId,
      OverallStatus = ProcessingStatus.Processing
    };

    _logger.LogWarning("Creating processing job with ID {id}", job.Id);

    var createdJob = await _repository.CreateAsync(job);
    _logger.LogInformation("Created processing job {JobId} for document {DocumentId}",
        createdJob.Id, input.DocumentId);

    return createdJob;
  }

  [Function("UpdateJobClassificationAsync")]
  public async Task UpdateJobClassificationAsync([ActivityTrigger] DocumentClassifiedMessage message)
  {
    _logger.LogInformation("Updating job classification for document {DocumentId} with type {DocumentType} and confidence {ConfidenceScore}",
        message.DocumentId, message.DocumentType, message.ConfidenceScore);

    try
    {
      var job = await _repository.GetByIdAndTenantIdAsync(message.DocumentId.ToString(), message.TenantId);
      if (job == null)
      {
        _logger.LogError("Job not found for document {DocumentId} during classification update", message.DocumentId);
        throw new InvalidOperationException($"Job not found for document {message.DocumentId}");
      }

      job.ClassificationResult = message.DocumentType;
      job.ClassificationConfidenceScore = message.ConfidenceScore;
      job.OverallStatus = ProcessingStatus.Classified;
      job.UpdatedAt = DateTime.UtcNow;

      await _repository.UpdateAsync(job);
      _logger.LogInformation("Successfully updated job {JobId} with classification result", job.Id);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to update job classification for document {DocumentId}", message.DocumentId);
      throw;
    }
  }

  [Function("UpdateJobExtractionAsync")]
  public async Task UpdateJobExtractionAsync([ActivityTrigger] DocumentExtractedMessage message)
  {
    _logger.LogInformation("Updating job extraction for document {DocumentId}", message.DocumentId);

    try
    {
      var job = await _repository.GetByIdAndTenantIdAsync(message.DocumentId.ToString(), message.TenantId);
      if (job == null)
      {
        _logger.LogError("Job not found for document {DocumentId} during extraction update", message.DocumentId);
        throw new InvalidOperationException($"Job not found for document {message.DocumentId}");
      }

      job.ExtractionResult = message.ParsedData;
      job.OverallStatus = ProcessingStatus.Extracted;
      job.UpdatedAt = DateTime.UtcNow;

      await _repository.UpdateAsync(job);
      _logger.LogInformation("Successfully updated job {JobId} with extraction result", job.Id);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to update job extraction for document {DocumentId}", message.DocumentId);
      throw;
    }
  }

  [Function("UpdateJobValidationAsync")]
  public async Task UpdateJobValidationAsync([ActivityTrigger] DocumentValidatedMessage message)
  {
    _logger.LogInformation("Updating job validation for document {DocumentId}", message.DocumentId);

    var job = await _repository.GetByIdAndTenantIdAsync(message.DocumentId.ToString(), message.TenantId);
    if (job == null)
    {
      _logger.LogError("Job not found for document {DocumentId} during validation update", message.DocumentId);
      throw new InvalidOperationException($"Job not found for document {message.DocumentId}");
    }
    try
    {
      job.ValidationResult = string.Join(", ", message.Issues.Select(i => $"{i.FieldName}: {i.Description} ({i.Severity})"));
      job.RequiresHumanReview = !message.IsValid;
      job.OverallStatus = message.IsValid ? ProcessingStatus.Validated : ProcessingStatus.PendingHumanReview;
      await _repository.UpdateAsync(job);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to update job validation for document {DocumentId}", message.DocumentId);
      throw;
    }

  }

  [Function("FailJobAsync")]
  public async Task FailJobAsync([ActivityTrigger] (string documentId, string tenantId, string errorMessage) input)
  {
    var job = await _repository.GetByIdAndTenantIdAsync(input.documentId, input.tenantId);
    if (job == null) return;

    job.ErrorMessage = input.errorMessage;
    job.OverallStatus = ProcessingStatus.Failed;
    await _repository.UpdateAsync(job);
  }
}

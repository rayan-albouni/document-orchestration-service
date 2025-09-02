using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using DocumentOrchestrationService.Domain.ValueObjects;

namespace DocumentOrchestrationService.Application.Orchestrators;

public class UpdateClassificationOrchestrator
{
    [Function("UpdateClassificationOrchestrator")]
    public async Task<string> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<UpdateClassificationOrchestrator>();
        var input = context.GetInput<DocumentClassifiedMessage>();
        if (input == null) 
        {
            logger.LogWarning("UpdateClassificationOrchestrator received null input");
            return "Invalid input";
        }

        logger.LogInformation("Updating classification for document {DocumentId}", input.DocumentId);

        try
        {
            await context.CallActivityAsync("UpdateJobClassificationAsync", input);
            logger.LogInformation("Successfully updated classification for document {DocumentId}", input.DocumentId);
            return "Classification updated successfully";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update classification for document {DocumentId}", input.DocumentId);
            return $"Classification update failed: {ex.Message}";
        }
    }
}

public class UpdateExtractionOrchestrator
{
    [Function("UpdateExtractionOrchestrator")]
    public async Task<string> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<UpdateExtractionOrchestrator>();
        var input = context.GetInput<DocumentExtractedMessage>();
        if (input == null) 
        {
            logger.LogWarning("UpdateExtractionOrchestrator received null input");
            return "Invalid input";
        }

        logger.LogInformation("Updating extraction for document {DocumentId}", input.DocumentId);

        try
        {
            await context.CallActivityAsync("UpdateJobExtractionAsync", input);
            logger.LogInformation("Successfully updated extraction for document {DocumentId}", input.DocumentId);
            return "Extraction updated successfully";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update extraction for document {DocumentId}", input.DocumentId);
            return $"Extraction update failed: {ex.Message}";
        }
    }
}

public class UpdateValidationOrchestrator
{
    [Function("UpdateValidationOrchestrator")]
    public async Task<string> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<UpdateValidationOrchestrator>();
        var input = context.GetInput<DocumentValidatedMessage>();
        if (input == null) 
        {
            logger.LogWarning("UpdateValidationOrchestrator received null input");
            return "Invalid input";
        }

        logger.LogInformation("Updating validation for document {DocumentId}", input.DocumentId);

        try
        {
            await context.CallActivityAsync("UpdateJobValidationAsync", input);
            logger.LogInformation("Successfully updated validation for document {DocumentId}", input.DocumentId);
            return "Validation updated successfully";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update validation for document {DocumentId}", input.DocumentId);
            return $"Validation update failed: {ex.Message}";
        }
    }
}

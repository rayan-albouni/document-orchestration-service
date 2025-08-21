using DocumentOrchestrationService.Domain.Repositories;
using DocumentOrchestrationService.Domain.Services;
using DocumentOrchestrationService.Infrastructure.Repositories;
using DocumentOrchestrationService.Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Services.AddApplicationInsightsTelemetryWorkerService();

var cosmosDbConnectionString = builder.Configuration["CosmosDbConnectionString"] ?? throw new InvalidOperationException("CosmosDbConnectionString is not configured.");
var cosmosDbDatabaseId = builder.Configuration["CosmosDbDatabaseId"] ?? throw new InvalidOperationException("CosmosDbDatabaseId is not configured.");
var cosmosDbContainerId = builder.Configuration["CosmosDbContainerId"] ?? throw new InvalidOperationException("CosmosDbContainerId is not configured.");

builder.Services.AddSingleton<IProcessingJobRepository, ProcessingJobRepository>(s => new ProcessingJobRepository(new CosmosClient(cosmosDbConnectionString), cosmosDbDatabaseId, cosmosDbContainerId, s.GetRequiredService<ILogger<ProcessingJobRepository>>()));
builder.Services.AddHttpClient<IDocumentClassificationService, DocumentClassificationService>();
builder.Services.AddHttpClient<IDocumentExtractionService, DocumentExtractionService>();
builder.Services.AddHttpClient<IDataValidationService, DataValidationService>();
builder.Services.AddHttpClient<IHumanReviewService, HumanReviewService>();
builder.Services.AddHttpClient<IProcessedDataService, ProcessedDataService>();

await builder.Build().RunAsync();
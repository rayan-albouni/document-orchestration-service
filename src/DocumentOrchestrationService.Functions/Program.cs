using System.Text.Json;
using Azure.Messaging.ServiceBus;
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
builder.Logging.AddApplicationInsights();
builder.Services.Configure<JsonSerializerOptions>(o => o.IncludeFields = true);

var cosmosDbConnectionString = builder.Configuration["CosmosDbConnectionString"] ?? throw new InvalidOperationException("CosmosDbConnectionString is not configured.");
var cosmosDbDatabaseId = builder.Configuration["CosmosDbDatabaseId"] ?? throw new InvalidOperationException("CosmosDbDatabaseId is not configured.");
var cosmosDbContainerId = builder.Configuration["CosmosDbContainerId"] ?? throw new InvalidOperationException("CosmosDbContainerId is not configured.");
var serviceBusConnectionString = builder.Configuration["ServiceBusConnectionString"] ?? throw new InvalidOperationException("ServiceBusConnectionString is not configured.");

// Register Cosmos DB
builder.Services.AddSingleton<IProcessingJobRepository, ProcessingJobRepository>(s => new ProcessingJobRepository(new CosmosClient(cosmosDbConnectionString), cosmosDbDatabaseId, cosmosDbContainerId, s.GetRequiredService<ILogger<ProcessingJobRepository>>()));

// Register Service Bus
builder.Services.AddSingleton<ServiceBusClient>(s => new ServiceBusClient(serviceBusConnectionString));
builder.Services.AddSingleton<IMessagingBusService, MessagingBusService>();

// Register HTTP clients for external services
builder.Services.AddHttpClient<IDocumentClassificationService, DocumentClassificationService>();
builder.Services.AddHttpClient<IDocumentExtractionService, DocumentExtractionService>();
builder.Services.AddHttpClient<IDataValidationService, DataValidationService>();
builder.Services.AddHttpClient<IHumanReviewService, HumanReviewService>();
builder.Services.AddHttpClient<IProcessedDataService, ProcessedDataService>();

await builder.Build().RunAsync();

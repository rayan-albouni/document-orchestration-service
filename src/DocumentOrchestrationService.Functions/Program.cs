using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using DocumentOrchestrationService.Domain.Repositories;
using DocumentOrchestrationService.Domain.Services;
using DocumentOrchestrationService.Infrastructure.Repositories;
using DocumentOrchestrationService.Infrastructure.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var connectionString = configuration["CosmosDbConnectionString"] ?? throw new InvalidOperationException("CosmosDbConnectionString is not configured.");
            return new CosmosClient(connectionString);
        });

        services.AddScoped<IProcessingJobRepository>(serviceProvider =>
        {
            var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
            var logger = serviceProvider.GetRequiredService<ILogger<ProcessingJobRepository>>();
            var databaseName = configuration["CosmosDbDatabaseName"] ?? throw new InvalidOperationException("CosmosDbDatabaseName is not configured.");
            return new ProcessingJobRepository(cosmosClient, databaseName, logger);
        });

        services.AddHttpClient<IDocumentClassificationService, DocumentClassificationService>();
        services.AddHttpClient<IDocumentExtractionService, DocumentExtractionService>();
        services.AddHttpClient<IDataValidationService, DataValidationService>();
        services.AddHttpClient<IHumanReviewService, HumanReviewService>();
        services.AddHttpClient<IProcessedDataService, ProcessedDataService>();
    })
    .Build();

host.Run();

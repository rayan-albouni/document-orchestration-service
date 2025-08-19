# Document Orchestration Service

Azure Durable Functions service that orchestrates document processing workflow.

## Solution Structure

The solution follows Clean Architecture principles with 4 separate projects:

### 📁 DocumentOrchestrationService.Domain
Contains core business logic and domain models:
- **Entities**: `ProcessingJob`, `ProcessingStatus`
- **Value Objects**: `DocumentMessage`, `DocumentStatusResponse`
- **Repository Interfaces**: `IProcessingJobRepository`
- **Service Interfaces**: External service contracts

### 📁 DocumentOrchestrationService.Application
Contains application logic and orchestration:
- **Orchestrators**: `DocumentProcessingOrchestrator`
- **Activities**: `ProcessingJobActivities`
- **Dependencies**: Domain project, Azure Functions Worker, DurableTask

### 📁 DocumentOrchestrationService.Infrastructure
Contains external concerns and implementations:
- **Repositories**: `ProcessingJobRepository` (CosmosDB)
- **Services**: HTTP client implementations for external APIs
- **Dependencies**: Domain project, CosmosDB, HTTP clients

### 📁 DocumentOrchestrationService.Functions
Contains Azure Functions entry points and configuration:
- **Functions**: `DocumentIngestionTrigger`, `DocumentStatusFunction`
- **Configuration**: `Program.cs`, `host.json`, `local.settings.json`
- **Dependencies**: Application and Infrastructure projects

## Architecture

The service follows Domain-Driven Design (DDD) principles with clean architecture:
- **Domain Layer**: Core business logic (no dependencies)
- **Application Layer**: Orchestration logic (depends on Domain)
- **Infrastructure Layer**: External integrations (depends on Domain)
- **Functions Layer**: API endpoints and triggers (depends on Application & Infrastructure)

## Building & Running

### Build the entire solution:
```bash
dotnet build DocumentOrchestrationService.sln
```

### Run the Functions:
```bash
cd src/DocumentOrchestrationService.Functions
func start
```

### Restore packages:
```bash
dotnet restore DocumentOrchestrationService.sln
```

## Features

- Service Bus trigger for document ingestion queue
- Durable Functions orchestration for document processing workflow
- CosmosDB integration for processing job state management
- HTTP endpoint for document status retrieval
- Integration with external microservices

## Workflow

1. Document message received from Service Bus queue
2. Create processing job in CosmosDB
3. Classify document using DocumentClassificationService
4. Extract data using DocumentExtractionService
5. Validate extracted data using DataValidationService
6. If human review required, create review task and wait for completion
7. Store processed data using ProcessedDataService
8. Update final status in CosmosDB

## Configuration

Required environment variables in `local.settings.json`:

- `ServiceBusConnectionString`: Azure Service Bus connection string
- `CosmosDbConnectionString`: Azure Cosmos DB connection string
- `CosmosDbDatabaseName`: Cosmos DB database name (default: DocumentProcessing)
- `DocumentClassificationServiceUrl`: Classification service endpoint
- `DocumentExtractionServiceUrl`: Extraction service endpoint
- `DataValidationServiceUrl`: Validation service endpoint
- `HumanReviewServiceUrl`: Human review service endpoint
- `ProcessedDataServiceUrl`: Processed data service endpoint

## API Endpoints

- `GET /api/v1/documents/{documentId}/status` - Get document processing status

## Deployment

Deploy the Functions project to Azure Functions with Durable Functions extension.

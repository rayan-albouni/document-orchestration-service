# Documents List API

## Overview
This API provides an optimized way to retrieve paginated lists of documents for a specific tenant. The implementation is designed for high performance by leveraging Cosmos DB's partition key optimization.

## Performance Optimizations
- **Partition-scoped queries**: Uses `tenantId` as partition key for optimal performance
- **No complex filtering**: Simple queries that only scan within the specific tenant partition
- **Continuation token pagination**: Efficient pagination without offset/limit overhead
- **Configurable page size**: Supports up to 100 documents per page

## API Endpoints

### GET `/api/v1/tenants/{tenantId}/documents`
Retrieves a paginated list of documents for the specified tenant.

#### Query Parameters
- `pageSize` (optional): Number of documents per page (default: 20, max: 100)
- `continuationToken` (optional): Token for next page pagination
- `sortBy` (optional): Field to sort by (`CreatedAt`, `UpdatedAt`, `DocumentType`, `Status`) (default: `CreatedAt`)
- `sortOrder` (optional): Sort direction (`asc` or `desc`) (default: `desc`)

#### Example Request
```http
GET /api/v1/tenants/tenant-123/documents?pageSize=20&sortBy=CreatedAt&sortOrder=desc
```

#### Response Format
```json
{
  "documents": [
    {
      "documentId": "doc-123",
      "tenantId": "tenant-123",
      "documentType": "Invoice",
      "sourceSystem": "ERP",
      "userId": "user-456",
      "clientReferenceId": "ref-789",
      "overallStatus": "Processed",
      "createdAt": "2025-09-08T10:30:00Z",
      "updatedAt": "2025-09-08T11:00:00Z",
      "requiresHumanReview": false,
      "errorMessage": null,
      "blobUrl": "https://storage.example.com/docs/doc-123.pdf",
      "classificationResult": "Invoice",
      "classificationConfidenceScore": 0.95
    }
  ],
  "pageCount": 20,
  "pageSize": 20,
  "continuationToken": "eyJ0b2tlbiI6InNhbXBsZSJ9",
  "hasMoreResults": true
}
```

## Frontend Integration

### Load More Pattern
```javascript
class DocumentsList {
  constructor() {
    this.documents = [];
    this.continuationToken = null;
    this.isLoading = false;
    this.hasMoreResults = true;
  }

  async loadInitialDocuments(tenantId, pageSize = 20) {
    this.isLoading = true;
    try {
      const response = await fetch(`/api/v1/tenants/${tenantId}/documents?pageSize=${pageSize}`);
      const data = await response.json();

      this.documents = data.documents;
      this.continuationToken = data.continuationToken;
      this.hasMoreResults = data.hasMoreResults;
    } finally {
      this.isLoading = false;
    }
  }

  async loadMoreDocuments(tenantId) {
    if (!this.hasMoreResults || this.isLoading) return;

    this.isLoading = true;
    try {
      const url = `/api/v1/tenants/${tenantId}/documents?continuationToken=${encodeURIComponent(this.continuationToken)}`;
      const response = await fetch(url);
      const data = await response.json();

      this.documents.push(...data.documents);
      this.continuationToken = data.continuationToken;
      this.hasMoreResults = data.hasMoreResults;
    } finally {
      this.isLoading = false;
    }
  }
}
```

### React Hook Example
```javascript
import { useState, useCallback } from 'react';

export function useDocumentsList(tenantId) {
  const [documents, setDocuments] = useState([]);
  const [continuationToken, setContinuationToken] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMoreResults, setHasMoreResults] = useState(true);

  const loadDocuments = useCallback(async (reset = false) => {
    if (isLoading) return;

    setIsLoading(true);
    try {
      const token = reset ? null : continuationToken;
      const url = `/api/v1/tenants/${tenantId}/documents${token ? `?continuationToken=${encodeURIComponent(token)}` : ''}`;

      const response = await fetch(url);
      const data = await response.json();

      setDocuments(prev => reset ? data.documents : [...prev, ...data.documents]);
      setContinuationToken(data.continuationToken);
      setHasMoreResults(data.hasMoreResults);
    } finally {
      setIsLoading(false);
    }
  }, [tenantId, continuationToken, isLoading]);

  const refresh = useCallback(() => {
    setContinuationToken(null);
    setHasMoreResults(true);
    loadDocuments(true);
  }, [loadDocuments]);

  return {
    documents,
    isLoading,
    hasMoreResults,
    loadMore: () => loadDocuments(),
    refresh
  };
}
```

## Performance Benefits
1. **Single-partition queries**: Only scans documents within the specific tenant partition
2. **No WHERE clause overhead**: Simple ORDER BY queries are highly optimized in Cosmos DB
3. **Continuation token pagination**: Avoids expensive OFFSET operations
4. **Minimal data transfer**: Only returns essential document summary fields
5. **Optimal for large datasets**: Performance remains consistent regardless of total document count per tenant

## Best Practices
- Always use the continuation token for pagination rather than implementing offset-based pagination
- Keep page sizes reasonable (20-50 documents) for optimal user experience
- Implement proper loading states in the frontend
- Cache the continuation token properly to avoid duplicate API calls

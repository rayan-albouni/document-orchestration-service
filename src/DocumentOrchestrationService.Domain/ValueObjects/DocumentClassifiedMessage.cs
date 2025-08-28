public record DocumentClassifiedMessage
{
    Guid DocumentId { get; set; }
    string DocumentType { get; set; }
    string BlobUrl { get; set; }
    double ConfidenceScore { get; set; }
}
public record DocumentToExtractMessage
{
    Guid DocumentId { get; set; }
    string DocumentType { get; set; }
    string BlobUrl { get; set; }
}

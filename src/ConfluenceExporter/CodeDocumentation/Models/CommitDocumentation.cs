namespace ConfluenceExporter.CodeDocumentation.Models;

public sealed class CommitDocumentation
{
    public required DateTimeOffset AuthorDate { get; init; }
    public required string AuthorName { get; init; }
    public required string AuthorEmail { get; init; }
    public required string Message { get; init; }
    public required string Hash { get; init; }
}

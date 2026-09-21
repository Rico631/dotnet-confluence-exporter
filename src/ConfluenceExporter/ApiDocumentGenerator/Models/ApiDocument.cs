using System.Diagnostics;

namespace ConfluenceExporter.ApiDocumentGenerator.Models;

[DebuggerDisplay($"{{{nameof(BaseUrl)}}} {{{nameof(Version)}}} {{{nameof(Title)}}}")]
public sealed class ApiDocument
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Version { get; init; }

    public string? BaseUrl { get; init; }

    public IReadOnlyCollection<ApiEndpoint> Endpoints { get; init; } = [];
}
namespace ConfluenceExporter.CodeDocumentation.Models;

public sealed class HandlerDocumentationRenderOptions
{
    public string? GitlabRepositoryUrl { get; init; }

    public string GitlabReference { get; init; } = "main";
}

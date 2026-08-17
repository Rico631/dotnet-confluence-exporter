using System.Diagnostics;

namespace AppDocs.CodeDocumentation.Models;

[DebuggerDisplay($"{{{nameof(Name)}}} {{{nameof(Description)}}}")]
public sealed class DomainEventDocumentation
{
    public required string Name { get; init; }

    public string? Description { get; init; }
}
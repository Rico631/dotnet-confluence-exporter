
using System.Diagnostics;

namespace ConfluenceExporter.CodeDocumentation.Models;

[DebuggerDisplay($"Name:{{{nameof(Name)}}} Description:{{{nameof(Description)}}} DeclaringType:{{{nameof(DeclaringType)}}}")]
public sealed class MethodDocumentation
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string DeclaringType { get; init; }
    public required string Signature { get; init; }
    public List<MethodDocumentation> Children { get; init; } = [];
}
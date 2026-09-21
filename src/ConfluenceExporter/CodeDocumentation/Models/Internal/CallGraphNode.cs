using Microsoft.CodeAnalysis;

namespace ConfluenceExporter.CodeDocumentation.Models.Internal;

public sealed class CallGraphNode
{
    public required IMethodSymbol Method { get; init; }

    public List<CallGraphNode> Children { get; } = [];
}


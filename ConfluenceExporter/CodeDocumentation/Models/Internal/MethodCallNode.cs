using Microsoft.CodeAnalysis;

namespace AppDocs.CodeDocumentation.Models.Internal;

public sealed class MethodCallNode
{
    /// <summary>
    /// Метод, соответствующий узлу дерева.
    /// </summary>
    public required IMethodSymbol Method { get; init; }

    /// <summary>
    /// Узлы методов, вызываемых из текущего метода.
    /// </summary>
    public List<MethodCallNode> Children { get; } = [];
}

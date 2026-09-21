
using System.Diagnostics;

namespace ConfluenceExporter.CodeDocumentation.Models;

[DebuggerDisplay($"Name:{{{nameof(Name)}}} FullName:{{{nameof(FullName)}}} Description:{{{nameof(Description)}}}")]
public sealed class TypeDocumentation
{
    /// <summary>
    /// Имя типа.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Полное имя типа.
    /// </summary>
    public string? FullName { get; init; }

    /// <summary>
    /// Описание типа.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Свойства типа.
    /// </summary>
    public List<PropertyDocumentation> Properties { get; init; } = [];
}
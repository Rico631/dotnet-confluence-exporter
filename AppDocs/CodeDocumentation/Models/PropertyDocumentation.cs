
using System.Diagnostics;

namespace AppDocs.CodeDocumentation.Models;

[DebuggerDisplay($"Name:{{{nameof(Name)}}} Type:{{{nameof(Type)}}} Description:{{{nameof(Description)}}}")]
public sealed class PropertyDocumentation
{
    /// <summary>
    /// Имя свойства.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Отображаемый тип свойства.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Описание свойства.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Документация типа свойства.
    /// </summary>
    public TypeDocumentation? TypeDocumentation { get; init; }
}
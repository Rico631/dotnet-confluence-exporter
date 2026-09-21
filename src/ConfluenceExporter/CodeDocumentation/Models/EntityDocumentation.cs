using System.Diagnostics;

namespace ConfluenceExporter.CodeDocumentation.Models;


[DebuggerDisplay($"Name:{{{nameof(Name)}}} Type:{{{nameof(Type)}}}")]
public sealed class EntityDocumentation
{
    /// <summary>
    /// Имя сущности.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Полное имя типа сущности.
    /// </summary>
    public required string Type { get; init; }

    public string? Description { get; set; }

}

///// <summary>
///// Операции, выполняемые над сущностью.
///// </summary>
//public EntityAccessType AccessType { get; init; }

///// <summary>
///// Методы, в которых происходит обращение к сущности.
///// </summary>
//public List<string> Methods { get; init; } = [];

//[Flags]
//public enum EntityAccessType
//{
//    None = 0,

//    /// <summary>
//    /// Чтение сущности.
//    /// </summary>
//    Read = 1,

//    /// <summary>
//    /// Создание сущности.
//    /// </summary>
//    Create = 2,

//    /// <summary>
//    /// Изменение сущности.
//    /// </summary>
//    Update = 4,

//    /// <summary>
//    /// Удаление сущности.
//    /// </summary>
//    Delete = 8
//}
using System.Diagnostics;

namespace AppDocs.CodeDocumentation.Models;

[DebuggerDisplay($"HandlerName:{{{nameof(HandlerName)}}} HandlerDescription:{{{nameof(HandlerDescription)}}}")]
public sealed class HandlerDocumentation
{
    public required string HandlerName { get; init; }

    public string? HandlerDescription { get; init; }

    /// <summary>
    /// История изменений файла
    /// </summary>
    public List<CommitDocumentation> Commits { get; init; } = [];

    /// <summary>
    /// Входящие данные
    /// </summary>
    public required TypeDocumentation Request { get; init; }

    /// <summary>
    /// Исходящие данные
    /// </summary>
    public required TypeDocumentation Response { get; init; }

    /// <summary>
    /// Проверка входящих данных
    /// </summary>
    public List<ValidationDocumentation> Validations { get; init; } = [];

    /// <summary>
    /// Порождаемые доменные события
    /// </summary>
    public List<DomainEventDocumentation> DomainEvents { get; init; } = [];

    /// <summary>
    /// Затрагиваемые сущности
    /// </summary>
    public List<EntityDocumentation> Entities { get; init; } = [];

    /// <summary>
    /// Дерево вызовов
    /// </summary>
    public List<MethodDocumentation> CallTree { get; init; } = [];
}
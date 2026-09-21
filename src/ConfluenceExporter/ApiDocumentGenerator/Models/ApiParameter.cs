using System.Diagnostics;

namespace ConfluenceExporter.ApiDocumentGenerator.Models;

[DebuggerDisplay($"{{{nameof(Name)}}} {{{nameof(Type)}}}")]
public sealed class ApiParameter
{
    /// <summary>
    /// Имя параметра.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Где находится параметр:
    /// path, query, header, cookie.
    /// </summary>
    public required ApiParameterLocation Location { get; init; }

    /// <summary>
    /// Тип параметра.
    /// Например string, integer, uuid, datetime.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Формат параметра.
    /// Например uuid, date-time, int64.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Обязательный ли параметр.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// Описание параметра.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Значение по умолчанию.
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Возможные значения enum.
    /// </summary>
    public IReadOnlyCollection<string> AllowedValues { get; init; } = [];
}

public enum ApiParameterLocation
{
    Path,
    Query,
    Header,
    Cookie
}
using System.Diagnostics;

namespace ConfluenceExporter.ApiDocumentGenerator.Models;

[DebuggerDisplay($"{{{nameof(Name)}}} {{{nameof(Type)}}}")]
public sealed class ApiSchema
{
    /// <summary>
    /// Название модели.
    /// Например UserCreateRequest.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Тип.
    /// object, array, string, integer...
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Format.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Описание.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Обязательное поле.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// Enum values.
    /// </summary>
    public IReadOnlyCollection<string> AllowedValues { get; init; } = [];

    /// <summary>
    /// Для object.
    /// </summary>
    public IReadOnlyCollection<ApiSchemaProperty> Properties { get; init; } = [];

    /// <summary>
    /// Для array.
    /// </summary>
    public ApiSchema? Items { get; init; }
}

[DebuggerDisplay($"{{{nameof(Name)}}} {{{nameof(Type)}}}")]

public sealed class ApiSchemaProperty
{
    public required string Name { get; init; }

    public string? Type { get; init; }

    public string? Format { get; init; }

    public string? Description { get; init; }

    public bool Required { get; init; }

    public ApiSchema? Schema { get; init; }
}
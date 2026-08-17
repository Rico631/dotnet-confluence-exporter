using System.Diagnostics;

namespace AppDocs.ApiDocumentGenerator.Models;

[DebuggerDisplay($"{{{nameof(Format)}}} {{{nameof(Type)}}}")]
public sealed class ApiRequest
{
    /// <summary>
    /// Content-Type.
    /// Например application/json.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Тип тела запроса.
    /// Например UserCreateRequest.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Формат тела запроса.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Обязательное ли тело запроса.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// Описание request body.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Структура тела запроса.
    /// </summary>
    public ApiSchema? Schema { get; init; }
}
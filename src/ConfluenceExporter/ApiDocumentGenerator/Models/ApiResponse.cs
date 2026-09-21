using System.Diagnostics;

namespace ConfluenceExporter.ApiDocumentGenerator.Models;

[DebuggerDisplay($"{{{nameof(StatusCode)}}} {{{nameof(Type)}}}")]
public sealed class ApiResponse
{
    /// <summary>
    /// HTTP status code.
    /// Например 200, 400, 404.
    /// </summary>
    public required string StatusCode { get; init; }

    /// <summary>
    /// Description из OpenAPI.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Content-Type.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Тип возвращаемого объекта.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Schema ответа.
    /// </summary>
    public ApiSchema? Schema { get; init; }

    /// <summary>
    /// Headers ответа.
    /// </summary>
    public IReadOnlyCollection<ApiParameter> Headers { get; init; } = [];
}
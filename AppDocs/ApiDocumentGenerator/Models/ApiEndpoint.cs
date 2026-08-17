using System.Diagnostics;

namespace AppDocs.ApiDocumentGenerator.Models;

[DebuggerDisplay($"{{{nameof(Method)}}} {{{nameof(Path)}}}")]
public sealed class ApiEndpoint
{
    /// <summary>
    /// HTTP method: GET, POST, PUT, DELETE...
    /// </summary>
    public required string Method { get; init; }

    /// <summary>
    /// API path, например /api/users/{id}.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Краткое описание endpoint.
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// Полное описание endpoint.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// OperationId из OpenAPI.
    /// </summary>
    public string? OperationId { get; init; }

    /// <summary>
    /// HTTP headers.
    /// </summary>
    public IReadOnlyCollection<ApiParameter> Headers { get; init; } = [];

    /// <summary>
    /// Path/query parameters.
    /// </summary>
    public IReadOnlyCollection<ApiParameter> Parameters { get; init; } = [];

    /// <summary>
    /// Request body.
    /// </summary>
    public ApiRequest? Request { get; init; }

    /// <summary>
    /// Возможные HTTP responses.
    /// </summary>
    public IReadOnlyCollection<ApiResponse> Responses { get; init; } = [];

    /// <summary>
    /// Tags из OpenAPI.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = [];

    /// <summary>
    /// Security requirements.
    /// </summary>
    public IReadOnlyCollection<string> SecuritySchemes { get; init; } = [];
}

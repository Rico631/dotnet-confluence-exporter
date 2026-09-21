using AppDocs.ApiDocumentGenerator.Models;
using Microsoft.OpenApi;
using System.Text;

namespace AppDocs.ApiDocumentGenerator;

public sealed class OpenApiParser
{
    public async Task<ApiDocument> ParseAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var result = await OpenApiDocument.LoadAsync(stream, cancellationToken: cancellationToken);

        if (result.Document is null)
            throw new InvalidOperationException(
                "Не удалось загрузить OpenAPI document.");

        return ParseDocument(result.Document);
    }

    public async Task<ApiDocument> ParseAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);

        return await ParseAsync(stream, cancellationToken);
    }

    public ApiDocument Parse(string json)
    {
        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(json));

        return ParseAsync(stream).GetAwaiter().GetResult();
    }

    private static ApiDocument ParseDocument(OpenApiDocument document)
    {
        var endpoints = new List<ApiEndpoint>();

        foreach (var path in document.Paths)
        {
            var pathTemplate = path.Key;
            var pathItem = path.Value;

            foreach (var operation in pathItem.Operations!)
            {
                var endpoint = ParseOperation(
                    pathTemplate,
                    operation.Key,
                    operation.Value);

                endpoints.Add(endpoint);
            }
        }

        return new ApiDocument
        {
            Title = document.Info?.Title,
            Description = document.Info?.Description,
            Version = document.Info?.Version,
            BaseUrl = GetBaseUrl(document),
            Endpoints = endpoints
        };
    }

    private static ApiEndpoint ParseOperation(
        string path,
        HttpMethod method,
        OpenApiOperation operation)
    {
        var parameters = new List<ApiParameter>();
        var headers = new List<ApiParameter>();

        foreach (var parameter in operation.Parameters ?? [])
        {
            var parsedParameter = ParseParameter(parameter);

            if (parsedParameter.Location == ApiParameterLocation.Header)
                headers.Add(parsedParameter);
            else
                parameters.Add(parsedParameter);
        }

        return new ApiEndpoint
        {
            Method = method.Method.ToUpperInvariant(),
            Path = path,

            Summary = operation.Summary,
            Description = operation.Description,
            OperationId = operation.OperationId,

            Tags = operation.Tags?
                .Select(x => x.Name!)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray()
                ?? [],

            Parameters = parameters,
            Headers = headers,

            Request = ParseRequest(operation.RequestBody),

            Responses = ParseResponses(operation.Responses),

            SecuritySchemes = ParseSecurity(operation.Security)
        };
    }

    private static ApiParameter ParseParameter(
        IOpenApiParameter parameter)
    {
        return new ApiParameter
        {
            Name = parameter.Name!,

            Location = ParseParameterLocation(
                parameter.In),

            Required = parameter.Required,

            Description = parameter.Description,

            Type = GetSchemaType(parameter.Schema),

            Format = GetSchemaFormat(parameter.Schema),

            DefaultValue = parameter.Schema?.Default?.ToString(),

            AllowedValues = GetEnumValues(parameter.Schema)
        };
    }

    private static ApiParameterLocation ParseParameterLocation(
        ParameterLocation? location)
    {
        return location switch
        {
            ParameterLocation.Path =>
                ApiParameterLocation.Path,

            ParameterLocation.Query =>
                ApiParameterLocation.Query,

            ParameterLocation.Header =>
                ApiParameterLocation.Header,

            ParameterLocation.Cookie =>
                ApiParameterLocation.Cookie,

            _ => throw new NotSupportedException(
                $"Unsupported parameter location: {location}")
        };
    }

    private static ApiRequest? ParseRequest(
        IOpenApiRequestBody? requestBody)
    {
        if (requestBody is null)
            return null;

        var content = requestBody.Content;

        if (content is null || content.Count == 0)
        {
            return new ApiRequest
            {
                Description = requestBody.Description,
                Required = requestBody.Required
            };
        }

        // Обычно выбираем application/json.
        var mediaType =
            content.FirstOrDefault(x =>
                x.Key.Equals(
                    "application/json",
                    StringComparison.OrdinalIgnoreCase))
            .Value
            ?? content.First().Value;

        return new ApiRequest
        {
            ContentType = content.FirstOrDefault(x =>
                    ReferenceEquals(x.Value, mediaType))
                .Key,

            Required = requestBody.Required,

            Description = requestBody.Description,

            Type = GetSchemaType(mediaType.Schema),

            Format = GetSchemaFormat(mediaType.Schema),

            Schema = ParseSchema(mediaType.Schema)
        };
    }

    private static IReadOnlyCollection<ApiResponse> ParseResponses(
        OpenApiResponses? responses)
    {
        if (responses is null)
            return [];

        var result = new List<ApiResponse>();

        foreach (var response in responses)
        {
            var responseValue = response.Value;

            var content = responseValue.Content;

            if (content is null || content.Count == 0)
            {
                result.Add(new ApiResponse
                {
                    StatusCode = response.Key,
                    Description = responseValue.Description
                });

                continue;
            }

            var mediaType =
                content.FirstOrDefault(x =>
                    x.Key.Equals(
                        "application/json",
                        StringComparison.OrdinalIgnoreCase))
                .Value
                ?? content.First().Value;

            var contentType =
                content.FirstOrDefault(x =>
                    ReferenceEquals(x.Value, mediaType))
                .Key;

            result.Add(new ApiResponse
            {
                StatusCode = response.Key,

                Description = responseValue.Description,

                ContentType = contentType,

                Type = GetSchemaType(mediaType.Schema),

                Schema = ParseSchema(mediaType.Schema)
            });
        }

        return result;
    }

    private static ApiSchema? ParseSchema(
        IOpenApiSchema? schema)
    {
        if (schema is null)
            return null;

        return ParseSchema(
            schema,
            new HashSet<IOpenApiSchema>(
                ReferenceEqualityComparer.Instance));
    }

    private static ApiSchema ParseSchema(
        IOpenApiSchema schema,
        HashSet<IOpenApiSchema> visited)
    {
        if (!visited.Add(schema))
        {
            return new ApiSchema
            {
                Name = GetSchemaName(schema),
                Type = GetSchemaType(schema),
                Format = GetSchemaFormat(schema),
                Description = schema.Description
            };
        }

        var properties = new List<ApiSchemaProperty>();

        foreach (var property in schema.Properties ?? new Dictionary<string, IOpenApiSchema>())
        {
            var propertySchema = property.Value;

            properties.Add(new ApiSchemaProperty
            {
                Name = property.Key,

                Type = GetSchemaType(propertySchema),

                Format = GetSchemaFormat(propertySchema),

                Description = propertySchema.Description,

                Required = schema.Required?
                    .Contains(property.Key)
                    ?? false,

                Schema = ParseSchema(
                    propertySchema,
                    visited)
            });
        }

        ApiSchema? items = null;

        if (schema.Items is not null)
        {
            items = ParseSchema(
                schema.Items,
                visited);
        }

        return new ApiSchema
        {
            Name = GetSchemaName(schema),

            Type = GetSchemaType(schema),

            Format = GetSchemaFormat(schema),

            Description = schema.Description,

            Required = false,

            AllowedValues = GetEnumValues(schema),

            Properties = properties,

            Items = items
        };
    }

    private static string? GetSchemaName(
        IOpenApiSchema? schema)
    {
        if (schema is null)
            return null;

        if (schema is OpenApiSchema openApiSchema)
        {
            if (!string.IsNullOrWhiteSpace(openApiSchema.Title))
                return openApiSchema.Title;
        }

        return null;
    }

    private static string? GetSchemaType(
        IOpenApiSchema? schema)
    {
        if (schema is null)
            return null;

        return schema.Type.ToString();
    }

    private static string? GetSchemaFormat(
        IOpenApiSchema? schema)
    {
        if (schema is null)
            return null;

        return schema.Format;
    }

    private static IReadOnlyCollection<string> GetEnumValues(
        IOpenApiSchema? schema)
    {
        if (schema?.Enum is null)
            return [];

        return schema.Enum
            .Select(x => x.ToString())
            .ToArray();
    }

    private static IReadOnlyCollection<string> ParseSecurity(
        IEnumerable<OpenApiSecurityRequirement>? security)
    {
        if (security is null)
            return [];

        return security
            .SelectMany(x => x.Keys)
            .Select(GetSecuritySchemeName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string GetSecuritySchemeName(
        IOpenApiSecurityScheme scheme)
    {
        return scheme.Scheme!;
    }

    private static string? GetBaseUrl(
        OpenApiDocument document)
    {
        var server = document.Servers?.FirstOrDefault();

        return server?.Url;
    }
}
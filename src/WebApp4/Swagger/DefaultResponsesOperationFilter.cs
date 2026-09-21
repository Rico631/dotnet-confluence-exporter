using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp4.Shared;

namespace WebApp4.Swagger;

public class DefaultResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses!.ContainsKey("400"))
            return;

        var schema = context.SchemaGenerator.GenerateSchema(typeof(ApiErrorResponse), context.SchemaRepository);

        operation.Responses["400"] = new OpenApiResponse
        {
            Description = "Bad Request",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = schema }
            }
        };
    }
}

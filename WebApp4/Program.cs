using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Add Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add XML comments for the main project assembly
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    options.IncludeXmlComments(xmlPath);

    // Add XML comments for the shared project assembly (WebApp4.Shared)
    var sharedXmlFilename = $"{typeof(WebApp4.Shared.Dpv.AddCartItemRequest).Assembly.GetName().Name}.xml";
    var sharedXmlPath = Path.Combine(AppContext.BaseDirectory, sharedXmlFilename);
    options.IncludeXmlComments(sharedXmlPath);

    // Add default responses operation filter that documents a 400 response for all endpoints
    options.OperationFilter<WebApp4.Swagger.DefaultResponsesOperationFilter>();

    // Define two Swagger documents: one for LK endpoints and one for DPV endpoints.
    options.SwaggerDoc("lk", new OpenApiInfo { Title = "LK API", Version = "v1" });
    options.SwaggerDoc("dpv", new OpenApiInfo { Title = "DPV API", Version = "v1" });

    // Only include actions in the document when the ApiExplorer group name matches the document name.
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.GroupName == null) return false;
        return apiDesc.GroupName.Equals(docName, StringComparison.OrdinalIgnoreCase);
    });
});

var app = builder.Build();

// Enable middleware to serve generated Swagger as JSON endpoints.
app.UseSwagger();

// Configure Swagger UI with endpoints for each document.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/lk/swagger.json", "LK API V1");
    c.SwaggerEndpoint("/swagger/dpv/swagger.json", "DPV API V1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

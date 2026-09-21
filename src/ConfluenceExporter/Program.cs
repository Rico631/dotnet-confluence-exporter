using ConfluenceExporter.ApiDocumentGenerator;
using ConfluenceExporter.CodeDocumentation.Analysis;
using ConfluenceExporter.CodeDocumentation.Models;
using ConfluenceExporter.CodeDocumentation.Renders;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebApp4.Application.Features.Dpv;

var handlerDoc = await DocumentationAnalyzer.AnalyzeAsync<AddCartItem>();

var render = new HandlerDocumentationRenderer();
var renderOptions = new HandlerDocumentationRenderOptions
{
    GitlabRepositoryUrl = "https://gitlab.company.ru/lti/c-lti/"
};
var xhtml = render.Render(handlerDoc[0], renderOptions);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var json = JsonSerializer.Serialize(handlerDoc, jsonOptions);
Console.WriteLine(json);

var openApiDocuments = await OpenApiCreator.Create();

var json2 = JsonSerializer.Serialize(openApiDocuments, jsonOptions);
Console.WriteLine(json2);

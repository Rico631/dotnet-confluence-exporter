using ConfluenceExporter.ApiDocumentGenerator.Models;

namespace ConfluenceExporter.ApiDocumentGenerator;

public static class OpenApiCreator
{

    public static async Task<List<ApiDocument>> Create()
    {
        var parser = new OpenApiParser();

        // Find the nearest 'openapi' directory by walking up parent directories from the application's base directory
        var dirInfo = new DirectoryInfo(AppContext.BaseDirectory);
        DirectoryInfo? openApiDir = null;
        while (dirInfo != null)
        {
            var candidate = Path.Combine(dirInfo.FullName, "openapi");
            if (Directory.Exists(candidate))
            {
                openApiDir = new DirectoryInfo(candidate);
                break;
            }
            dirInfo = dirInfo.Parent;
        }

        if (openApiDir == null)
        {
            Console.Error.WriteLine("openapi directory not found.");
            return [];
        }

        var files = Directory.GetFiles(openApiDir.FullName, "*.json");

        List<ApiDocument> apiDocuments = [];


        foreach (var filePath in files)
        {
            Console.WriteLine($"Parsing: {filePath}");

            var document = await parser.ParseAsync(filePath);
            apiDocuments.Add(document);
        }

        return apiDocuments;
    }
}


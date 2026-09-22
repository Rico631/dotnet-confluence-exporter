using ConfluenceExporter.CodeDocumentation.Models;
using Microsoft.CodeAnalysis.MSBuild;
using System.Diagnostics;

namespace ConfluenceExporter.CodeDocumentation.Analysis;

public static class DocumentationAnalyzer
{
    public static async Task<IReadOnlyList<HandlerDocumentation>> AnalyzeAsync<T>(AnalysisOptions? options = null)
    {
        var analysisOptions = options ?? new AnalysisOptions();
        var assembly = typeof(T).Assembly;

        var assemblyName = assembly.GetName().Name;

        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            throw new InvalidOperationException(
                $"Unable to determine assembly name for '{typeof(T).FullName}'.");
        }

        var solutionPath = FindSolutionFile();

        using var workspace = MSBuildWorkspace.Create();

        using var workspaceFailedRegistration = workspace.RegisterWorkspaceFailedHandler(args =>
        {
            var msg = $"Workspace failure: {args.Diagnostic.Kind} - {args.Diagnostic.Message}";
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        });

        var solution = await workspace.OpenSolutionAsync(solutionPath);

        var project = solution.Projects
            .FirstOrDefault(x =>
                string.Equals(
                    x.AssemblyName,
                    assemblyName,
                    StringComparison.OrdinalIgnoreCase));

        if (project is null)
        {
            throw new InvalidOperationException(
                $"Project with assembly name '{assemblyName}' " +
                $"was not found in solution '{solutionPath}'.");
        }

        Console.WriteLine($"Project: {project.Name}");
        Console.WriteLine($"FilePath: {project.FilePath}");
        Console.WriteLine($"AssemblyName: {project.AssemblyName}");
        Console.WriteLine($"Language: {project.Language}");
        Console.WriteLine($"Documents: {project.Documents.Count()}");
        Console.WriteLine($"HasCompilation: {project.SupportsCompilation}");

        if (!project.SupportsCompilation)
        {
            throw new InvalidOperationException(
                $"Project '{project.Name}' does not support compilation.");
        }

        var compilation = await project.GetCompilationAsync();

        if (compilation is null)
        {
            throw new InvalidOperationException(
                $"Failed to create compilation for project '{project.Name}'.");
        }

        var collector = new HandlerCollector(compilation, analysisOptions.RequestHandlerName);

        return collector.Collect();
    }

    private static string FindSolutionFile()
    {
        var directory = new DirectoryInfo(
            AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionFiles = directory
                .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Where(x =>
                    x.Extension.Equals(
                        ".sln",
                        StringComparison.OrdinalIgnoreCase) ||
                    x.Extension.Equals(
                        ".slnx",
                        StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (solutionFiles.Length == 1)
                return solutionFiles[0].FullName;

            if (solutionFiles.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple solution files found in " +
                    $"'{directory.FullName}'.");
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Could not find solution starting from " +
            $"'{AppContext.BaseDirectory}'.");
    }
}
using AwesomeAssertions;
using ConfluenceExporter.CodeDocumentation.Analysis;
using ConfluenceExporter.CodeDocumentation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ConfluenceExporter.Tests;

public sealed class DocumentationAnalyzerTests
{
    [Fact]
    public void HandlerCollector_WhenCustomRequestHandlerNameIsProvided_UsesIt()
    {
        var compilation = CreateCompilation("""
            namespace Sample;

            public interface IEntity { }
            public interface IDomainEvent { }

            public interface ICommandHandler<TRequest, TResponse>
            {
            }

            public sealed class AddItemCommand
            {
            }

            public sealed class AddItemResponse
            {
            }

            public sealed class AddItemHandler : ICommandHandler<AddItemCommand, AddItemResponse>
            {
            }
            """);

        var collector = new HandlerCollector(compilation, "ICommandHandler");

        var handlers = collector.Collect();

        handlers.Should().ContainSingle();
        handlers[0].HandlerName.Should().Be("Sample.AddItemHandler");
    }

    [Fact]
    public void HandlerCollector_WhenNoCustomNameIsProvided_UsesDefaultIRequestHandler()
    {
        var compilation = CreateCompilation("""
            namespace Sample;

            public interface IEntity { }
            public interface IDomainEvent { }

            public interface IRequestHandler<TRequest, TResponse>
            {
            }

            public sealed class AddItemRequest
            {
            }

            public sealed class AddItemResponse
            {
            }

            public sealed class AddItemHandler : IRequestHandler<AddItemRequest, AddItemResponse>
            {
            }
            """);

        var collector = new HandlerCollector(compilation);

        var handlers = collector.Collect();

        handlers.Should().ContainSingle();
        handlers[0].HandlerName.Should().Be("Sample.AddItemHandler");
    }

    private static Compilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
                .Select(x => MetadataReference.CreateFromFile(x.Location))
                .ToArray(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}

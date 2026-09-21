using Microsoft.CodeAnalysis;

namespace ConfluenceExporter.CodeDocumentation.Analysis;

using ConfluenceExporter.CodeDocumentation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

public sealed class DomainEventCollector(
    Compilation compilation,
    string domainEventInterfaceMetadataName = "IDomainEvent")
{
    private readonly INamedTypeSymbol _domainEventInterface =
            compilation.GetTypeByMetadataName(domainEventInterfaceMetadataName)
            ?? FindTypeByName(compilation, domainEventInterfaceMetadataName)
            ?? throw new InvalidOperationException(
                $"Unable to find domain event interface '{domainEventInterfaceMetadataName}'.");

    public IReadOnlyList<DomainEventDocumentation> Collect(
    IReadOnlyCollection<IMethodSymbol> methods)
    {
        var events = new Dictionary<
            INamedTypeSymbol,
            DomainEventDocumentation>(
            SymbolEqualityComparer.Default);

        foreach (var method in methods)
        {
            CollectMethod(
                method,
                events);
        }

        return events.Values
            .OrderBy(x => x.Name)
            .ToList();
    }

    private void CollectMethod(
        IMethodSymbol method,
        Dictionary<INamedTypeSymbol, DomainEventDocumentation> events)
    {
        foreach (var syntaxReference in method.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();

            var semanticModel =
                compilation.GetSemanticModel(syntax.SyntaxTree);

            var operation =
                semanticModel.GetOperation(syntax);

            if (operation is null)
                continue;

            foreach (var op in operation.DescendantsAndSelf())
            {
                if (op is not IObjectCreationOperation creation)
                    continue;

                if (creation.Type is not INamedTypeSymbol type)
                    continue;

                if (!IsDomainEvent(type))
                    continue;

                AddDomainEvent(
                    type,
                    events);
            }
        }
    }


    private void AddDomainEvent(
        INamedTypeSymbol type,
        Dictionary<INamedTypeSymbol, DomainEventDocumentation> events)
    {
        type = type.OriginalDefinition;

        if (events.ContainsKey(type))
            return;

        events[type] = new DomainEventDocumentation
        {
            Name = type.Name,
            Description = GetSummary(type)
        };
    }

    private bool IsDomainEvent(
        INamedTypeSymbol type)
    {
        return type.AllInterfaces.Any(x =>
            SymbolEqualityComparer.Default.Equals(
                x.OriginalDefinition,
                _domainEventInterface.OriginalDefinition));
    }

    private static INamedTypeSymbol? FindTypeByName(
        Compilation compilation,
        string name)
    {
        return GetAllTypes(compilation.Assembly.GlobalNamespace)
            .FirstOrDefault(x =>
                x.Name == name);
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(
        INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;

            foreach (var nestedType in GetNestedTypes(type))
                yield return nestedType;
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(childNamespace))
                yield return type;
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(
        INamedTypeSymbol type)
    {
        foreach (var nestedType in type.GetTypeMembers())
        {
            yield return nestedType;

            foreach (var childType in GetNestedTypes(nestedType))
                yield return childType;
        }
    }

    private static string? GetSummary(
        ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml();

        if (string.IsNullOrWhiteSpace(xml))
            return null;

        return XmlDocumentationParser.GetSummary(xml);
    }
}
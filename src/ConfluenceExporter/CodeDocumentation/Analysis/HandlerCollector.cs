using ConfluenceExporter.CodeDocumentation.Models;
using ConfluenceExporter.CodeDocumentation.Models.Internal;
using Microsoft.CodeAnalysis;

namespace ConfluenceExporter.CodeDocumentation.Analysis;

public sealed class HandlerCollector(Compilation compilation, string requestHandlerName = "IRequestHandler")
{
    private readonly ValidationCollector _validationCollector = new(compilation);
    private readonly EntityCollector _entityCollector = new(compilation);
    private readonly DomainEventCollector _domainEventCollector = new(compilation);
    private readonly CallGraphBuilder _callGraphBuilder = new(compilation);
    private readonly CommitDocumentationCollector _commitCollector = new();

    public IReadOnlyList<HandlerDocumentation> Collect()
    {
        return GetAllTypes(compilation.Assembly.GlobalNamespace)
            .Where(IsRequestHandler)
            .Select(CreateHandlerDocumentation)
            .ToList();
    }

    private HandlerDocumentation CreateHandlerDocumentation(
        INamedTypeSymbol handler)
    {
        var requestHandler = handler.AllInterfaces
            .First(IsRequestHandlerInterface);

        var requestType = requestHandler.TypeArguments[0];
        var responseType = requestHandler.TypeArguments[1];

        var methods = _callGraphBuilder.Build(handler);

        return new HandlerDocumentation
        {
            HandlerName = handler.ToDisplayString(),
            HandlerDescription = GetSummary(handler),
            Commits = CollectCommits(handler).ToList(),
            Request = CreateTypeDocumentation(requestType),
            Response = CreateTypeDocumentation(responseType),
            Validations = _validationCollector.Collect(requestType).ToList(),
            Entities = _entityCollector.Collect(methods).ToList(),
            DomainEvents = _domainEventCollector.Collect(methods).ToList(),
            CallTree = _callGraphBuilder.BuildTree(handler).Select(ToDocumentation).ToList()
        };
    }

    private IReadOnlyList<CommitDocumentation> CollectCommits(
        INamedTypeSymbol handler)
    {
        var filePath = handler.Locations
            .FirstOrDefault(x => x.IsInSource)?
            .SourceTree?
            .FilePath;

        if (string.IsNullOrWhiteSpace(filePath))
            return [];

        return _commitCollector.Collect(filePath);
    }

    private TypeDocumentation CreateTypeDocumentation(
        ITypeSymbol type)
    {
        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        return CreateTypeDocumentation(type, visited);
    }

    private TypeDocumentation CreateTypeDocumentation(
        ITypeSymbol type,
        HashSet<ITypeSymbol> visited)
    {
        var documentation = new TypeDocumentation
        {
            Name = type.Name,
            FullName = type.ToDisplayString(),
            Description = GetSummary(type)
        };

        if (type is not INamedTypeSymbol namedType)
            return documentation;

        // Защита от циклических ссылок:
        //
        // A -> B -> A
        //
        if (!visited.Add(type))
            return documentation;

        foreach (var property in GetProperties(namedType))
        {
            if (property.Name == "EqualityContract")
                continue;

            documentation.Properties.Add(
                CreatePropertyDocumentation(property, visited));
        }

        return documentation;
    }

    private PropertyDocumentation CreatePropertyDocumentation(
        IPropertySymbol property,
        HashSet<ITypeSymbol> visited)
    {
        var propertyType = property.Type;

        return new PropertyDocumentation
        {
            Name = property.Name,
            Type = propertyType.ToDisplayString(),
            Description = GetSummary(property),
            TypeDocumentation = ShouldDocumentType(propertyType)
                ? CreateTypeDocumentation(propertyType, visited)
                : null
        };
    }

    private static bool ShouldDocumentType(ITypeSymbol type)
    {
        if (type.SpecialType != SpecialType.None)
            return false;

        if (type.TypeKind == TypeKind.Enum)
            return false;

        if (type.IsValueType)
            return false;

        return type is INamedTypeSymbol;
    }

    private static IEnumerable<IPropertySymbol> GetProperties(
        INamedTypeSymbol type)
    {
        return type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(x =>
                !x.IsStatic &&
                x.GetMethod is not null &&
                x.DeclaredAccessibility != Accessibility.Private);
    }

    private bool IsRequestHandler(
        INamedTypeSymbol type)
    {
        return type.AllInterfaces.Any(IsRequestHandlerInterface);
    }

    private bool IsRequestHandlerInterface(
        INamedTypeSymbol @interface)
    {
        return @interface.IsGenericType &&
               @interface.ConstructedFrom.Name == requestHandlerName &&
               @interface.TypeArguments.Length == 2;
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

    private static string? GetSummary(ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml();

        if (string.IsNullOrWhiteSpace(xml))
            return null;

        return XmlDocumentationParser.GetSummary(xml);
    }

    private static string? GetMethodSummary(
    IMethodSymbol method)
    {
        // Сначала documentation самой реализации.
        var summary = GetSummary(method);

        if (!string.IsNullOrWhiteSpace(summary))
            return summary;

        // Затем documentation интерфейсного метода.
        foreach (var interfaceMethod
                 in GetImplementedInterfaceMethods(method))
        {
            summary = GetSummary(interfaceMethod);

            if (!string.IsNullOrWhiteSpace(summary))
                return summary;
        }

        return null;
    }

    private static IEnumerable<IMethodSymbol>
    GetImplementedInterfaceMethods(
        IMethodSymbol implementation)
    {
        var containingType =
            implementation.ContainingType;

        foreach (var @interface in containingType.AllInterfaces)
        {
            foreach (var interfaceMethod in @interface
                         .GetMembers()
                         .OfType<IMethodSymbol>())
            {
                var resolved =
                    containingType
                        .FindImplementationForInterfaceMember(
                            interfaceMethod);

                if (resolved is not IMethodSymbol resolvedMethod)
                    continue;

                if (SymbolEqualityComparer.Default.Equals(
                        resolvedMethod.OriginalDefinition,
                        implementation.OriginalDefinition))
                {
                    yield return interfaceMethod;
                }
            }
        }
    }

    private static MethodDocumentation ToDocumentation(
        CallGraphNode node)
    {
        return new MethodDocumentation
        {
            Name = node.Method.Name,

            Description =
                GetMethodSummary(node.Method),

            DeclaringType =
                node.Method.ContainingType.ToDisplayString(),

            Signature =
                node.Method.ToDisplayString(),

            Children = node.Children
                .Select(ToDocumentation)
                .ToList()
        };
    }
}
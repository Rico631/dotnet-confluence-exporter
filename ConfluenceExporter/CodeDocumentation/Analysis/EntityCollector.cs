using AppDocs.CodeDocumentation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace AppDocs.CodeDocumentation.Analysis;


public sealed class EntityCollector
{
    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol _entityInterface;

    public EntityCollector(
        Compilation compilation,
        string entityInterfaceMetadataName = "IEntity")
    {
        _compilation = compilation;

        _entityInterface =
            compilation.GetTypeByMetadataName(entityInterfaceMetadataName)
            ?? FindTypeByName(compilation, entityInterfaceMetadataName)
            ?? throw new InvalidOperationException(
                $"Unable to find entity interface '{entityInterfaceMetadataName}'.");
    }


    public IReadOnlyList<EntityDocumentation> Collect(
        IReadOnlyCollection<IMethodSymbol> methods)
    {
        var entities = new Dictionary<
            INamedTypeSymbol,
            EntityDocumentation>(
            SymbolEqualityComparer.Default);


        foreach (var method in methods)
        {
            CollectMethodSignature(
                method,
                entities);

            CollectMethodOperations(
                method,
                entities);
        }

        return entities.Values
            .OrderBy(x => x.Name)
            .ToList();
    }

    private void CollectMethod(
        IMethodSymbol method,
        Dictionary<INamedTypeSymbol, EntityDocumentation> entities,
        HashSet<IMethodSymbol> visitedMethods)
    {
        method = method.OriginalDefinition;

        if (!visitedMethods.Add(method))
            return;

        // Сигнатура метода тоже является частью того,
        // какие сущности затрагивает метод.
        CollectType(method.ReturnType, entities);

        foreach (var parameter in method.Parameters)
        {
            CollectType(parameter.Type, entities);
        }

        foreach (var syntaxReference in method.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();

            var semanticModel =
                _compilation.GetSemanticModel(syntax.SyntaxTree);

            var operation = semanticModel.GetOperation(syntax);

            if (operation is null)
                continue;

            foreach (var op in operation.DescendantsAndSelf())
            {
                CollectOperationTypes(
                    op,
                    entities);

                CollectCalledMethod(
                    op,
                    entities,
                    visitedMethods);
            }
        }
    }

    private void CollectCalledMethod(
        IOperation operation,
        Dictionary<INamedTypeSymbol, EntityDocumentation> entities,
        HashSet<IMethodSymbol> visitedMethods)
    {
        switch (operation)
        {
            case IInvocationOperation invocation:
                {
                    var method = invocation.TargetMethod;

                    CollectMethod(
                        method,
                        entities,
                        visitedMethods);

                    break;
                }

            case IObjectCreationOperation creation:
                {
                    var constructor = creation.Constructor;

                    if (constructor is not null)
                    {
                        CollectMethod(
                            constructor,
                            entities,
                            visitedMethods);
                    }

                    break;
                }

            case IMethodReferenceOperation methodReference:
                {
                    CollectMethod(
                        methodReference.Method,
                        entities,
                        visitedMethods);

                    break;
                }
        }
    }

    private void CollectOperationTypes(
        IOperation operation,
        Dictionary<INamedTypeSymbol, EntityDocumentation> entities)
    {
        // Общий тип операции.
        //
        // Например:
        //   cart.CartItems
        //
        // type = List<CartItem>
        //
        // CollectType рекурсивно разберёт List<CartItem>
        // и найдёт CartItem.
        CollectType(
            operation.Type,
            entities);

        switch (operation)
        {
            case IObjectCreationOperation creation:
                CollectType(
                    creation.Type,
                    entities);

                if (creation.Constructor is not null)
                {
                    CollectMethodSignature(
                        creation.Constructor,
                        entities);
                }

                break;

            case IInvocationOperation invocation:
                CollectMethodSignature(
                    invocation.TargetMethod,
                    entities);

                break;

            case IMethodReferenceOperation methodReference:
                CollectMethodSignature(
                    methodReference.Method,
                    entities);

                break;

            case IPropertyReferenceOperation propertyReference:
                CollectType(
                    propertyReference.Property.ContainingType,
                    entities);

                CollectType(
                    propertyReference.Property.Type,
                    entities);

                break;

            case IFieldReferenceOperation fieldReference:
                CollectType(
                    fieldReference.Field.ContainingType,
                    entities);

                CollectType(
                    fieldReference.Field.Type,
                    entities);

                break;

            case ILocalReferenceOperation localReference:
                CollectType(
                    localReference.Local.Type,
                    entities);

                break;

            case IParameterReferenceOperation parameterReference:
                CollectType(
                    parameterReference.Parameter.Type,
                    entities);

                break;

            case IInstanceReferenceOperation instanceReference:
                CollectType(
                    instanceReference.Type,
                    entities);

                break;

            case IEventReferenceOperation eventReference:
                CollectType(
                    eventReference.Event.ContainingType,
                    entities);

                CollectType(
                    eventReference.Event.Type,
                    entities);

                break;

            case IIsTypeOperation isType:
                CollectType(
                    isType.TypeOperand,
                    entities);

                break;

            case ITypeOfOperation typeOf:
                CollectType(
                    typeOf.TypeOperand,
                    entities);

                break;
        }
    }

    private void CollectMethodSignature(
        IMethodSymbol method,
        Dictionary<INamedTypeSymbol, EntityDocumentation> entities)
    {
        CollectType(
            method.ContainingType,
            entities);

        CollectType(
            method.ReturnType,
            entities);

        foreach (var parameter in method.Parameters)
        {
            CollectType(
                parameter.Type,
                entities);
        }
    }

    private void CollectMethodOperations(
    IMethodSymbol method,
    Dictionary<INamedTypeSymbol, EntityDocumentation> entities)
    {
        foreach (var syntaxReference in method.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();

            var semanticModel =
                _compilation.GetSemanticModel(syntax.SyntaxTree);

            var operation =
                semanticModel.GetOperation(syntax);

            if (operation is null)
                continue;

            foreach (var op in operation.DescendantsAndSelf())
            {
                CollectOperationTypes(
                    op,
                    entities);
            }
        }
    }

    private void CollectType(
        ITypeSymbol? type,
        Dictionary<INamedTypeSymbol, EntityDocumentation> entities)
    {
        if (type is null)
            return;

        switch (type)
        {
            case INamedTypeSymbol namedType:
                {
                    // Сама сущность.
                    if (IsEntity(namedType))
                    {
                        AddEntity(
                            namedType,
                            entities);

                        return;
                    }

                    // Например:
                    //
                    // List<CartItem>
                    // Dictionary<Guid, Cart>
                    //
                    // Нужно рекурсивно пройти generic arguments.
                    foreach (var typeArgument in namedType.TypeArguments)
                    {
                        CollectType(
                            typeArgument,
                            entities);
                    }

                    break;
                }

            case IArrayTypeSymbol arrayType:
                {
                    CollectType(
                        arrayType.ElementType,
                        entities);

                    break;
                }

            case IPointerTypeSymbol pointerType:
                {
                    CollectType(
                        pointerType.PointedAtType,
                        entities);

                    break;
                }

            case IDynamicTypeSymbol:
                break;
        }
    }

    private void AddEntity(
        INamedTypeSymbol entity,
        Dictionary<INamedTypeSymbol, EntityDocumentation> entities)
    {
        entity = entity.OriginalDefinition;

        if (entities.ContainsKey(entity))
            return;

        entities[entity] = new EntityDocumentation
        {
            Name = entity.Name,
            Type = entity.ToDisplayString(),
            Description = GetSummary(entity)
        };
    }

    private bool IsEntity(INamedTypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class
            && !type.IsAbstract
            && type.AllInterfaces.Any(x =>
                SymbolEqualityComparer.Default.Equals(
                    x.OriginalDefinition,
                    _entityInterface.OriginalDefinition));
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

            foreach (var nested in GetNestedTypes(type))
                yield return nested;
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
using AppDocs.CodeDocumentation.Models.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace AppDocs.CodeDocumentation.Analysis;

public sealed class CallGraphBuilder
{
    private readonly Compilation _compilation;

    public CallGraphBuilder(
        Compilation compilation)
    {
        _compilation = compilation;
    }

    public IReadOnlyCollection<IMethodSymbol> Build(
        INamedTypeSymbol handler)
    {
        var methods = new HashSet<IMethodSymbol>(
            SymbolEqualityComparer.Default);

        var visited = new HashSet<IMethodSymbol>(
            SymbolEqualityComparer.Default);

        foreach (var handleMethod in GetHandleMethods(handler))
        {
            CollectMethod(
                handleMethod,
                methods,
                visited);
        }

        return methods;
    }

    public IReadOnlyList<CallGraphNode> BuildTree(
        INamedTypeSymbol handler)
    {
        var result = new List<CallGraphNode>();

        var handleMethods = GetHandleMethods(handler)
            .ToList();

        foreach (var handleMethod in handleMethods)
        {
            var node = BuildTreeNode(
                handleMethod,
                new HashSet<IMethodSymbol>(
                    SymbolEqualityComparer.Default));

            if (node is not null)
            {
                result.Add(node);
            }
        }

        return result;
    }

    private CallGraphNode? BuildTreeNode(
        IMethodSymbol method,
        HashSet<IMethodSymbol> path)
    {
        method = method.OriginalDefinition;

        // Защита от циклов именно в текущем пути.
        //
        // В отличие от глобального visited это позволяет:
        //
        // A -> B
        // A -> C
        //
        // и B/C могут независимо использовать один и тот же метод.
        if (!path.Add(method))
            return null;

        var node = new CallGraphNode
        {
            Method = method
        };

        foreach (var syntaxReference in method.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();

            var semanticModel =
                _compilation.GetSemanticModel(syntax.SyntaxTree);

            var operation =
                semanticModel.GetOperation(syntax);

            if (operation is null)
                continue;

            foreach (var operationNode in operation.DescendantsAndSelf())
            {
                foreach (var target in ResolveOperationTargets(
                             operationNode))
                {
                    var child = BuildTreeNode(
                        target,
                        new HashSet<IMethodSymbol>(
                            path,
                            SymbolEqualityComparer.Default));

                    if (child is not null)
                    {
                        AddChild(
                            node,
                            child);
                    }
                }
            }
        }

        return node;
    }

    private static void AddChild(
        CallGraphNode parent,
        CallGraphNode child)
    {
        if (parent.Children.Any(x =>
                SymbolEqualityComparer.Default.Equals(
                    x.Method,
                    child.Method)))
        {
            return;
        }

        parent.Children.Add(child);
    }

    private IEnumerable<IMethodSymbol> ResolveOperationTargets(
        IOperation operation)
    {
        switch (operation)
        {
            case IInvocationOperation invocation:
                {
                    return ResolveInvocationTargets(invocation);
                }

            case IObjectCreationOperation objectCreation:
                {
                    if (objectCreation.Constructor is IMethodSymbol constructor)
                    {
                        return [constructor];
                    }

                    return [];
                }

            case IMethodReferenceOperation methodReference:
                {
                    return ResolveMethodReferenceTargets(
                        methodReference);
                }

            default:
                return [];
        }
    }

    private void CollectMethod(
        IMethodSymbol method,
        HashSet<IMethodSymbol> methods,
        HashSet<IMethodSymbol> visited)
    {
        method = method.OriginalDefinition;

        if (!visited.Add(method))
            return;

        methods.Add(method);

        foreach (var syntaxReference in method.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();

            var semanticModel =
                _compilation.GetSemanticModel(syntax.SyntaxTree);

            var operation =
                semanticModel.GetOperation(syntax);

            if (operation is null)
                continue;

            foreach (var operationNode in operation.DescendantsAndSelf())
            {
                foreach (var target in ResolveOperationTargets(
                             operationNode))
                {
                    CollectMethod(
                        target,
                        methods,
                        visited);
                }
            }
        }
    }

    private IEnumerable<IMethodSymbol> ResolveInvocationTargets(
        IInvocationOperation invocation)
    {
        var targetMethod = invocation.TargetMethod;

        if (targetMethod is null)
            yield break;

        var containingType = targetMethod.ContainingType;

        if (containingType is null)
            yield break;

        // Interface call:
        //
        // _service.Create()
        //
        // TargetMethod:
        //     IOrderService.Create
        //
        // Нужно вернуть:
        //     OrderService.Create
        //
        // а не оба метода.
        if (containingType.TypeKind == TypeKind.Interface)
        {
            var implementations =
                ResolveInterfaceInvocationTargets(invocation)
                    .ToList();

            if (implementations.Count > 0)
            {
                foreach (var implementation in implementations)
                    yield return implementation;

                yield break;
            }

            // Если реализацию определить не удалось,
            // оставляем интерфейсный метод.
            yield return targetMethod;

            yield break;
        }

        // Обычный вызов конкретного метода.
        yield return targetMethod;

        // Virtual / abstract / override.
        if (targetMethod.IsVirtual ||
            targetMethod.IsAbstract ||
            targetMethod.IsOverride)
        {
            var receiverType =
                invocation.Instance?.Type as INamedTypeSymbol;

            if (receiverType is not null &&
                receiverType.TypeKind == TypeKind.Class)
            {
                var implementation =
                    FindOverride(
                        receiverType,
                        targetMethod);

                if (implementation is not null &&
                    !SymbolEqualityComparer.Default.Equals(
                        implementation,
                        targetMethod))
                {
                    yield return implementation;
                }
            }
        }
    }

    private IEnumerable<IMethodSymbol>
        ResolveInterfaceInvocationTargets(
            IInvocationOperation invocation)
    {
        var interfaceMethod =
            invocation.TargetMethod;

        var receiverType =
            invocation.Instance?.Type as INamedTypeSymbol;

        if (receiverType is null)
            yield break;

        // Например:
        //
        // Order order;
        // order.Raise(...);
        //
        // receiverType = Order
        //
        if (receiverType.TypeKind == TypeKind.Class)
        {
            var implementation =
                FindInterfaceImplementation(
                    receiverType,
                    interfaceMethod);

            if (implementation is not null)
                yield return implementation;

            yield break;
        }

        // Например:
        //
        // IEntity entity;
        // entity.Raise(...);
        //
        // receiverType = IEntity
        //
        // Конкретный runtime type неизвестен.
        if (receiverType.TypeKind == TypeKind.Interface)
        {
            foreach (var type in GetAllTypes(
                         _compilation.Assembly.GlobalNamespace))
            {
                if (!IsConcreteClass(type))
                    continue;

                if (!ImplementsInterface(
                        type,
                        receiverType))
                {
                    continue;
                }

                var implementation =
                    FindInterfaceImplementation(
                        type,
                        interfaceMethod);

                if (implementation is not null)
                    yield return implementation;
            }
        }
    }

    private IEnumerable<IMethodSymbol>
        ResolveMethodReferenceTargets(
            IMethodReferenceOperation methodReference)
    {
        var targetMethod =
            methodReference.Method;

        yield return targetMethod;

        var containingType =
            targetMethod.ContainingType;

        if (containingType is null)
            yield break;

        if (containingType.TypeKind == TypeKind.Interface)
        {
            var receiverType =
                methodReference.Instance?.Type
                    as INamedTypeSymbol;

            if (receiverType is not null &&
                receiverType.TypeKind == TypeKind.Class)
            {
                var implementation =
                    FindInterfaceImplementation(
                        receiverType,
                        targetMethod);

                if (implementation is not null)
                {
                    yield return implementation;
                }

                yield break;
            }

            if (receiverType is not null &&
                receiverType.TypeKind == TypeKind.Interface)
            {
                foreach (var type in GetAllTypes(
                             _compilation.Assembly.GlobalNamespace))
                {
                    if (!IsConcreteClass(type))
                        continue;

                    if (!ImplementsInterface(
                            type,
                            receiverType))
                    {
                        continue;
                    }

                    var implementation =
                        FindInterfaceImplementation(
                            type,
                            targetMethod);

                    if (implementation is not null)
                    {
                        yield return implementation;
                    }
                }
            }

            yield break;
        }

        if (targetMethod.IsVirtual ||
            targetMethod.IsAbstract ||
            targetMethod.IsOverride)
        {
            var receiverType =
                methodReference.Instance?.Type
                    as INamedTypeSymbol;

            if (receiverType is not null &&
                receiverType.TypeKind == TypeKind.Class)
            {
                var implementation =
                    FindOverride(
                        receiverType,
                        targetMethod);

                if (implementation is not null)
                {
                    yield return implementation;
                }
            }
        }
    }

    private static IMethodSymbol?
        FindInterfaceImplementation(
            INamedTypeSymbol receiverType,
            IMethodSymbol interfaceMethod)
    {
        if (receiverType.TypeKind != TypeKind.Class)
            return null;

        var implementation =
            receiverType.FindImplementationForInterfaceMember(
                interfaceMethod);

        return implementation as IMethodSymbol;
    }

    private static bool ImplementsInterface(
        INamedTypeSymbol type,
        INamedTypeSymbol interfaceType)
    {
        return type.AllInterfaces.Any(
            implementedInterface =>
                SymbolEqualityComparer.Default.Equals(
                    implementedInterface.OriginalDefinition,
                    interfaceType.OriginalDefinition));
    }

    private static IMethodSymbol? FindOverride(
        INamedTypeSymbol receiverType,
        IMethodSymbol baseMethod)
    {
        if (receiverType.TypeKind != TypeKind.Class)
            return null;

        foreach (var method in receiverType
                     .GetMembers(baseMethod.Name)
                     .OfType<IMethodSymbol>())
        {
            if (!IsSameSignature(
                    method,
                    baseMethod))
            {
                continue;
            }

            if (method.IsOverride)
                return method;
        }

        if (receiverType.BaseType is not null)
        {
            return FindOverride(
                receiverType.BaseType,
                baseMethod);
        }

        return null;
    }

    private static bool IsSameSignature(
        IMethodSymbol left,
        IMethodSymbol right)
    {
        if (!string.Equals(
                left.Name,
                right.Name,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (left.Parameters.Length != right.Parameters.Length)
            return false;

        for (var i = 0; i < left.Parameters.Length; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(
                    left.Parameters[i].Type,
                    right.Parameters[i].Type))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<IMethodSymbol> GetHandleMethods(
        INamedTypeSymbol handler)
    {
        foreach (var @interface in handler.AllInterfaces)
        {
            if (!IsRequestHandlerInterface(@interface))
                continue;

            foreach (var interfaceMethod in @interface
                         .GetMembers()
                         .OfType<IMethodSymbol>())
            {
                var implementation =
                    handler.FindImplementationForInterfaceMember(
                        interfaceMethod);

                if (implementation is IMethodSymbol method)
                {
                    yield return method;
                }
            }
        }
    }

    private static bool IsRequestHandlerInterface(
        INamedTypeSymbol @interface)
    {
        return @interface.IsGenericType
            && @interface.ConstructedFrom.Name == "IRequestHandler"
            && @interface.TypeArguments.Length == 2;
    }

    private static bool IsConcreteClass(
        INamedTypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class
            && !type.IsAbstract;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(
        INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;

            foreach (var nested in GetNestedTypes(type))
            {
                yield return nested;
            }
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(childNamespace))
            {
                yield return type;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(
        INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;

            foreach (var child in GetNestedTypes(nested))
            {
                yield return child;
            }
        }
    }
}
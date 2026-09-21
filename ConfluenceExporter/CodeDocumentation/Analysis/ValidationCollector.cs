namespace AppDocs.CodeDocumentation.Analysis;

using AppDocs.CodeDocumentation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ValidationCollector
{
    private const string AbstractValidatorName = "AbstractValidator";
    private const string RuleForName = "RuleFor";
    private const string WithMessageName = "WithMessage";

    private readonly Compilation _compilation;

    public ValidationCollector(Compilation compilation)
    {
        _compilation = compilation;
    }

    public IReadOnlyList<ValidationDocumentation> Collect(
        ITypeSymbol requestType)
    {
        var validators = GetAllTypes(_compilation.Assembly.GlobalNamespace)
            .Where(x => IsValidatorFor(x, requestType));

        return validators
            .SelectMany(CollectValidator)
            .ToList();
    }

    private IEnumerable<ValidationDocumentation> CollectValidator(
        INamedTypeSymbol validator)
    {
        foreach (var constructor in validator.InstanceConstructors)
        {
            foreach (var syntaxReference in constructor.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax()
                    is not ConstructorDeclarationSyntax constructorSyntax)
                {
                    continue;
                }

                var semanticModel = _compilation.GetSemanticModel(
                    constructorSyntax.SyntaxTree);

                foreach (var invocation in constructorSyntax
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>())
                {
                    var documentation = TryCreateValidation(
                        invocation,
                        semanticModel);

                    if (documentation is not null)
                        yield return documentation;
                }
            }
        }
    }

    private static ValidationDocumentation? TryCreateValidation(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        if (!IsRuleForInvocation(invocation, semanticModel))
            return null;

        if (invocation.ArgumentList.Arguments.Count != 1)
            return null;

        var propertyExpression =
            invocation.ArgumentList.Arguments[0].Expression;

        var property = GetPropertyPath(propertyExpression);

        if (string.IsNullOrWhiteSpace(property))
            return null;

        var validator = FindValidator(invocation);

        if (validator is null)
            return null;

        var message = FindMessage(invocation);

        return new ValidationDocumentation
        {
            Property = property,
            Validator = validator,
            Message = message
        };
    }

    private static bool IsRuleForInvocation(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        if (invocation.Expression is not IdentifierNameSyntax identifier)
            return false;

        if (!string.Equals(
                identifier.Identifier.Text,
                RuleForName,
                StringComparison.Ordinal))
        {
            return false;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(invocation);

        return symbolInfo.Symbol is IMethodSymbol method &&
               string.Equals(
                   method.Name,
                   RuleForName,
                   StringComparison.Ordinal);
    }

    private static string? FindValidator(
        InvocationExpressionSyntax ruleFor)
    {
        var current = ruleFor.Parent;

        while (current is not null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                var methodName = GetMethodName(invocation);

                if (methodName is not null &&
                    !string.Equals(
                        methodName,
                        WithMessageName,
                        StringComparison.Ordinal))
                {
                    return BuildValidatorName(invocation);
                }
            }

            current = current.Parent;
        }

        return null;
    }

    private static string BuildValidatorName(
        InvocationExpressionSyntax invocation)
    {
        var methodName = GetMethodName(invocation)!;

        var arguments = invocation.ArgumentList.Arguments
            .Select(x => x.Expression.ToString())
            .ToList();

        if (arguments.Count == 0)
            return methodName;

        return $"{methodName}({string.Join(", ", arguments)})";
    }

    private static string? FindMessage(
        InvocationExpressionSyntax ruleFor)
    {
        var current = ruleFor.Parent;

        while (current is not null)
        {
            if (current is InvocationExpressionSyntax invocation &&
                string.Equals(
                    GetMethodName(invocation),
                    WithMessageName,
                    StringComparison.Ordinal))
            {
                var argument = invocation.ArgumentList.Arguments
                    .FirstOrDefault();

                if (argument?.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(
                        SyntaxKind.StringLiteralExpression))
                {
                    return literal.Token.ValueText;
                }

                return argument?.Expression.ToString();
            }

            current = current.Parent;
        }

        return null;
    }

    private static string? GetMethodName(
        InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier
                => identifier.Identifier.Text,

            MemberAccessExpressionSyntax memberAccess
                => memberAccess.Name.Identifier.Text,

            GenericNameSyntax generic
                => generic.Identifier.Text,

            _ => null
        };
    }

    private static string? GetPropertyPath(
        ExpressionSyntax expression)
    {
        if (expression is not LambdaExpressionSyntax lambda)
            return null;

        if (lambda.Body is not ExpressionSyntax body)
            return null;

        return GetMemberPath(body);
    }

    private static string? GetMemberPath(
        ExpressionSyntax expression)
    {
        var parts = new Stack<string>();

        while (expression is MemberAccessExpressionSyntax memberAccess)
        {
            parts.Push(memberAccess.Name.Identifier.Text);
            expression = memberAccess.Expression;
        }

        if (expression is not IdentifierNameSyntax)
            return null;

        return string.Join(".", parts);
    }

    private static bool IsValidatorFor(
        INamedTypeSymbol type,
        ITypeSymbol requestType)
    {
        var current = type;

        while (current is not null)
        {
            if (IsAbstractValidator(current, requestType))
                return true;

            current = current.BaseType;
        }

        return false;
    }

    private static bool IsAbstractValidator(
        INamedTypeSymbol type,
        ITypeSymbol requestType)
    {
        if (!type.IsGenericType)
            return false;

        if (!string.Equals(
                type.Name,
                AbstractValidatorName,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (type.TypeArguments.Length != 1)
            return false;

        return SymbolEqualityComparer.Default.Equals(
            type.TypeArguments[0],
            requestType);
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
}
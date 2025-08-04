using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.SelectionSets;

/// <summary>
/// Legacy field collector - now delegates to DefaultFieldCollector for consistency.
/// 
/// DEPRECATED: Use IFieldCollector and DefaultFieldCollector directly for extensible directive handling.
/// This class exists only for backward compatibility with validation rules.
/// </summary>
[Obsolete("Use IFieldCollector and DefaultFieldCollector for extensible directive support. This will be removed in a future version.")]
public static class FieldCollector
{

    public static IReadOnlyDictionary<string, List<FieldSelection>> CollectFields(
        ISchema schema,
        ExecutableDocument document,
        ObjectDefinition objectDefinition,
        SelectionSet selectionSet,
        IReadOnlyDictionary<string, object?>? coercedVariableValues = null,
        List<string>? visitedFragments = null,
        IRuleVisitorContext? context = null)
    {
        // Use the new extensible system when context with services is available
        if (context?.RequestServices != null)
        {
            var fieldCollector = new DefaultFieldCollector(context.RequestServices);
            var result = fieldCollector.CollectFields(
                schema,
                document,
                objectDefinition,
                selectionSet,
                coercedVariableValues,
                visitedFragments);

            return result.Fields;
        }

        // Fallback: Create minimal service provider for backward compatibility
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IDirectiveHandler>("skip", new SkipDirectiveHandler());
        services.AddKeyedSingleton<IDirectiveHandler>("include", new IncludeDirectiveHandler());
        using var serviceProvider = services.BuildServiceProvider();

        var fallbackCollector = new DefaultFieldCollector(serviceProvider);
        var fallbackResult = fallbackCollector.CollectFields(
            schema,
            document,
            objectDefinition,
            selectionSet,
            coercedVariableValues,
            visitedFragments);

        return fallbackResult.Fields;
    }

}
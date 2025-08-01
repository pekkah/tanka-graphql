using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

/// <summary>
/// Default implementation of IFieldCollector with extensible directive handling
/// </summary>
public class DefaultFieldCollector : IFieldCollector
{
    private static readonly IReadOnlyDictionary<string, FragmentDefinition>
        Empty = new Dictionary<string, FragmentDefinition>(0);

    private readonly IServiceProvider _serviceProvider;

    public DefaultFieldCollector(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReadOnlyDictionary<string, List<FieldSelection>> CollectFields(
        ISchema schema,
        ExecutableDocument document,
        ObjectDefinition objectDefinition,
        SelectionSet selectionSet,
        IReadOnlyDictionary<string, object?>? coercedVariableValues = null,
        List<string>? visitedFragments = null)
    {
        visitedFragments ??= new List<string>();

        IReadOnlyDictionary<string, FragmentDefinition> fragments =
            document.FragmentDefinitions?.ToDictionary(f => f.FragmentName.Value, f => f)
            ?? Empty;

        var groupedFields = new Dictionary<string, List<FieldSelection>>();
        foreach (ISelection selection in selectionSet)
        {
            var directives = Enumerable.ToList<Directive>(GetDirectives(selection));

            // Process directives with handlers
            bool includeSelection = true;
            foreach (var directive in directives)
            {
                var handler = _serviceProvider.GetKeyedService<IDirectiveHandler>(directive.Name.Value);
                if (handler != null)
                {
                    var context = new DirectiveContext
                    {
                        Schema = schema,
                        ObjectDefinition = objectDefinition,
                        Selection = selection,
                        Directive = directive,
                        CoercedVariableValues = coercedVariableValues
                    };

                    var result = handler.Handle(context);
                    if (result.Handled && !result.Include)
                    {
                        includeSelection = false;
                        break;
                    }
                }
            }

            if (!includeSelection)
                continue;

            if (selection is FieldSelection fieldSelection)
            {
                Name name = fieldSelection.AliasOrName;
                if (!groupedFields.ContainsKey(name))
                    groupedFields[name] = new List<FieldSelection>();

                groupedFields[name].Add(fieldSelection);
            }

            if (selection is FragmentSpread fragmentSpread)
            {
                Name fragmentSpreadName = fragmentSpread.FragmentName;

                if (visitedFragments.Contains(fragmentSpreadName)) continue;

                visitedFragments.Add(fragmentSpreadName);

                if (!fragments.TryGetValue(fragmentSpreadName, out FragmentDefinition? fragment))
                    continue;

                NamedType fragmentTypeAst = fragment.TypeCondition;
                TypeDefinition? fragmentType = Ast.UnwrapAndResolveType(schema, fragmentTypeAst);

                if (!DoesFragmentTypeApply(objectDefinition, fragmentType))
                    continue;

                SelectionSet fragmentSelectionSet = fragment.SelectionSet;
                IReadOnlyDictionary<string, List<FieldSelection>> fragmentGroupedFieldSet = CollectFields(
                    schema,
                    document,
                    objectDefinition,
                    fragmentSelectionSet,
                    coercedVariableValues,
                    visitedFragments);

                foreach (KeyValuePair<string, List<FieldSelection>> fragmentGroup in fragmentGroupedFieldSet)
                {
                    string responseKey = fragmentGroup.Key;

                    if (!groupedFields.ContainsKey(responseKey))
                        groupedFields[responseKey] = new List<FieldSelection>();

                    groupedFields[responseKey].AddRange(fragmentGroup.Value);
                }
            }

            if (selection is InlineFragment inlineFragment)
            {
                NamedType? fragmentTypeAst = inlineFragment.TypeCondition;
                TypeDefinition? fragmentType = Ast.UnwrapAndResolveType(schema, fragmentTypeAst);

                if (fragmentType != null && !DoesFragmentTypeApply(objectDefinition, fragmentType))
                    continue;

                SelectionSet fragmentSelectionSet = inlineFragment.SelectionSet;
                IReadOnlyDictionary<string, List<FieldSelection>> fragmentGroupedFieldSet = CollectFields(
                    schema,
                    document,
                    objectDefinition,
                    fragmentSelectionSet,
                    coercedVariableValues,
                    visitedFragments);

                foreach (KeyValuePair<string, List<FieldSelection>> fragmentGroup in fragmentGroupedFieldSet)
                {
                    string responseKey = fragmentGroup.Key;

                    if (!groupedFields.ContainsKey(responseKey))
                        groupedFields[responseKey] = new List<FieldSelection>();

                    groupedFields[responseKey].AddRange(fragmentGroup.Value);
                }
            }
        }

        return groupedFields;
    }

    private static bool DoesFragmentTypeApply(ObjectDefinition objectDefinition, TypeDefinition? fragmentType)
    {
        if (fragmentType is null)
            return false;

        return Ast.DoesFragmentTypeApply(objectDefinition, fragmentType);
    }

    private static IEnumerable<Directive> GetDirectives(ISelection selection)
    {
        return selection.Directives ?? Language.Nodes.Directives.None;
    }
}
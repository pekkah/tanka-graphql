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

    public FieldCollectionResult CollectFields(
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
        var fieldMetadata = new Dictionary<string, IReadOnlyDictionary<string, object>>();
        foreach (ISelection selection in selectionSet)
        {
            var (includeSelection, selectionMetadata) = ProcessDirectives(
                selection, schema, objectDefinition, coercedVariableValues, false);

            if (!includeSelection)
                continue;

            if (selection is FieldSelection fieldSelection)
            {
                Name name = fieldSelection.AliasOrName;
                if (!groupedFields.ContainsKey(name))
                    groupedFields[name] = new List<FieldSelection>();

                groupedFields[name].Add(fieldSelection);

                // Store metadata for this field if any directive metadata was collected
                if (selectionMetadata.Any())
                {
                    fieldMetadata[name] = selectionMetadata;
                }
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
                FieldCollectionResult fragmentResult = CollectFields(
                    schema,
                    document,
                    objectDefinition,
                    fragmentSelectionSet,
                    coercedVariableValues,
                    visitedFragments);

                // Process directives on the fragment spread itself
                var (includeFragmentSpread, fragmentSpreadMetadata) = ProcessDirectives(
                    fragmentSpread, schema, objectDefinition, coercedVariableValues, true);

                if (!includeFragmentSpread)
                    continue;

                foreach (KeyValuePair<string, List<FieldSelection>> fragmentGroup in fragmentResult.Fields)
                {
                    string responseKey = fragmentGroup.Key;

                    if (!groupedFields.ContainsKey(responseKey))
                        groupedFields[responseKey] = new List<FieldSelection>();

                    groupedFields[responseKey].AddRange(fragmentGroup.Value);

                    // Start with fragment spread metadata (e.g., @defer on fragment spread)
                    var combinedMetadata = new Dictionary<string, object>(fragmentSpreadMetadata);

                    // Merge fragment field metadata if any
                    if (fragmentResult.FieldMetadata?.TryGetValue(responseKey, out var fragmentMetadata) == true)
                    {
                        foreach (var (key, value) in fragmentMetadata)
                        {
                            combinedMetadata[key] = value;
                        }
                    }

                    // Merge with existing field metadata if any
                    if (fieldMetadata.TryGetValue(responseKey, out var existingMetadata))
                    {
                        // Merge metadata - fragment spread metadata takes precedence for directives like @defer
                        var mergedMetadata = new Dictionary<string, object>(existingMetadata);
                        foreach (var (key, value) in combinedMetadata)
                        {
                            mergedMetadata[key] = value;
                        }
                        fieldMetadata[responseKey] = mergedMetadata;
                    }
                    else if (combinedMetadata.Any())
                    {
                        fieldMetadata[responseKey] = combinedMetadata;
                    }
                }
            }

            if (selection is InlineFragment inlineFragment)
            {
                NamedType? fragmentTypeAst = inlineFragment.TypeCondition;
                TypeDefinition? fragmentType = Ast.UnwrapAndResolveType(schema, fragmentTypeAst);

                if (fragmentType != null && !DoesFragmentTypeApply(objectDefinition, fragmentType))
                    continue;

                SelectionSet fragmentSelectionSet = inlineFragment.SelectionSet;
                FieldCollectionResult fragmentResult = CollectFields(
                    schema,
                    document,
                    objectDefinition,
                    fragmentSelectionSet,
                    coercedVariableValues,
                    visitedFragments);

                foreach (KeyValuePair<string, List<FieldSelection>> fragmentGroup in fragmentResult.Fields)
                {
                    string responseKey = fragmentGroup.Key;

                    if (!groupedFields.ContainsKey(responseKey))
                        groupedFields[responseKey] = new List<FieldSelection>();

                    groupedFields[responseKey].AddRange(fragmentGroup.Value);

                    // Merge fragment metadata if any, including directive metadata from inline fragment
                    var combinedMetadata = new Dictionary<string, object>();

                    // Add directive metadata from the inline fragment itself
                    if (selectionMetadata.Any())
                    {
                        foreach (var (key, value) in selectionMetadata)
                        {
                            combinedMetadata[key] = value;
                        }
                    }

                    // Add metadata from fields within the fragment
                    if (fragmentResult.FieldMetadata?.TryGetValue(responseKey, out var fragmentMetadata) == true)
                    {
                        foreach (var (key, value) in fragmentMetadata)
                        {
                            combinedMetadata[key] = value;
                        }
                    }

                    if (combinedMetadata.Any())
                    {
                        if (fieldMetadata.TryGetValue(responseKey, out var existingMetadata))
                        {
                            // Merge with existing metadata
                            var mergedMetadata = new Dictionary<string, object>(existingMetadata);
                            foreach (var (key, value) in combinedMetadata)
                            {
                                mergedMetadata[key] = value;
                            }
                            fieldMetadata[responseKey] = mergedMetadata;
                        }
                        else
                        {
                            fieldMetadata[responseKey] = combinedMetadata;
                        }
                    }
                }
            }
        }

        return new FieldCollectionResult
        {
            Fields = groupedFields,
            FieldMetadata = fieldMetadata.Any() ? fieldMetadata : null
        };
    }

    private (bool ShouldInclude, Dictionary<string, object> Metadata) ProcessDirectives(
        ISelection selection,
        ISchema schema,
        ObjectDefinition objectDefinition,
        IReadOnlyDictionary<string, object?>? coercedVariableValues,
        bool handleUnknownDirectives)
    {
        var directives = Enumerable.ToList<Directive>(GetDirectives(selection));
        var metadata = new Dictionary<string, object>();
        bool shouldInclude = true;

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
                if (result.Handled)
                {
                    if (!result.Include)
                    {
                        shouldInclude = false;
                        break;
                    }

                    // Collect metadata from directive handlers
                    if (result.Metadata != null)
                    {
                        foreach (var (key, value) in result.Metadata)
                        {
                            metadata[key] = value;
                        }
                    }
                }
            }
            else if (handleUnknownDirectives)
            {
                // Store unknown directive for later processing (like @defer)
                metadata[directive.Name] = directive;
            }
        }

        return (shouldInclude, metadata);
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
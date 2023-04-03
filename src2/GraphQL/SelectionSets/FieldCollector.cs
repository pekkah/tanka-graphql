using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public static class FieldCollector
{
    private static readonly IReadOnlyDictionary<string, FragmentDefinition>
        Empty = new Dictionary<string, FragmentDefinition>(0);

    public const string SkipDirectiveName = "skip";
    public const string IncludeDirectiveName = "include";

    public static IReadOnlyDictionary<string, List<FieldSelection>> CollectFields(
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

            Directive? skipDirective = directives.FirstOrDefault(d => d.Name == SkipDirectiveName);
            if (SkipSelection(skipDirective, coercedVariableValues, schema, objectDefinition, selection))
                continue;

            Directive? includeDirective = directives.FirstOrDefault(d => d.Name == IncludeDirectiveName);
            if (!IncludeSelection(includeDirective, coercedVariableValues, schema, objectDefinition, selection))
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

    private static bool GetIfArgumentValue(
        ISchema schema,
        Directive directive,
        IReadOnlyDictionary<string, object?>? coercedVariableValues,
        Argument? argument)
    {
        if (argument is null)
            return false;

        return Ast.GetIfArgumentValue(directive, coercedVariableValues, argument);
    }

    private static bool IncludeSelection(
        Directive? includeDirective,
        IReadOnlyDictionary<string, object?>? coercedVariableValues,
        ISchema schema,
        ObjectDefinition objectDefinition,
        object selection)
    {
        if (includeDirective?.Arguments == null)
            return true;

        Argument? ifArgument = includeDirective.Arguments.SingleOrDefault(a => a.Name == "if");
        return GetIfArgumentValue(schema, includeDirective, coercedVariableValues, ifArgument);
    }

    private static bool SkipSelection(
        Directive? skipDirective,
        IReadOnlyDictionary<string, object?>? coercedVariableValues,
        ISchema schema,
        ObjectDefinition objectDefinition,
        object selection)
    {
        if (skipDirective?.Arguments == null)
            return false;

        Argument? ifArgument = skipDirective.Arguments.SingleOrDefault(a => a.Name == "if");
        return GetIfArgumentValue(schema, skipDirective, coercedVariableValues, ifArgument);
    }
}
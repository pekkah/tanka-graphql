using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution;

public static class SelectionSets
{
    public const string SkipDirectiveName = "skip";
    public const string IncludeDirectiveName = "include";

    public static SelectionSet MergeSelectionSets(IReadOnlyCollection<FieldSelection> fields)
    {
        var selectionSet = new List<ISelection>();
        foreach (var field in fields)
        {
            var fieldSelectionSet = field.SelectionSet;
            if (fieldSelectionSet is null || fieldSelectionSet.Count == 0) continue;

            selectionSet.AddRange(fieldSelectionSet);
        }

        return new SelectionSet(selectionSet);
    }

    public static IReadOnlyDictionary<string, List<FieldSelection>> CollectFields(
        ISchema schema,
        ExecutableDocument document,
        ObjectDefinition objectDefinition,
        SelectionSet selectionSet,
        IReadOnlyDictionary<string, object?> coercedVariableValues,
        List<string>? visitedFragments = null)
    {
        visitedFragments ??= new List<string>();

        var fragments = document.FragmentDefinitions;

        var groupedFields = new Dictionary<string, List<FieldSelection>>();
        foreach (var selection in selectionSet)
        {
            var directives = GetDirectives(selection).ToList();

            var skipDirective = directives.FirstOrDefault(d => d.Name == SkipDirectiveName);
            if (SkipSelection(skipDirective, coercedVariableValues, schema, objectDefinition, selection))
                continue;

            var includeDirective = directives.FirstOrDefault(d => d.Name == IncludeDirectiveName);
            if (!IncludeSelection(includeDirective, coercedVariableValues, schema, objectDefinition, selection))
                continue;

            if (selection is FieldSelection fieldSelection)
            {
                var name = fieldSelection.AliasOrName;
                if (!groupedFields.ContainsKey(name))
                    groupedFields[name] = new List<FieldSelection>();

                groupedFields[name].Add(fieldSelection);
            }

            if (selection is FragmentSpread fragmentSpread)
            {
                var fragmentSpreadName = fragmentSpread.FragmentName;

                if (visitedFragments.Contains(fragmentSpreadName)) continue;

                visitedFragments.Add(fragmentSpreadName);

                var fragment = fragments.SingleOrDefault(f => f.FragmentName == fragmentSpreadName);

                if (fragment == null)
                    continue;

                var fragmentTypeAst = fragment.TypeCondition;
                var fragmentType = Ast.UnwrapAndResolveType(schema, fragmentTypeAst);

                if (!DoesFragmentTypeApply(objectDefinition, fragmentType))
                    continue;

                var fragmentSelectionSet = fragment.SelectionSet;
                var fragmentGroupedFieldSet = CollectFields(
                    schema,
                    document,
                    objectDefinition,
                    fragmentSelectionSet,
                    coercedVariableValues,
                    visitedFragments);

                foreach (var fragmentGroup in fragmentGroupedFieldSet)
                {
                    var responseKey = fragmentGroup.Key;

                    if (!groupedFields.ContainsKey(responseKey))
                        groupedFields[responseKey] = new List<FieldSelection>();

                    groupedFields[responseKey].AddRange(fragmentGroup.Value);
                }
            }

            if (selection is InlineFragment inlineFragment)
            {
                var fragmentTypeAst = inlineFragment.TypeCondition;
                var fragmentType = Ast.UnwrapAndResolveType(schema, fragmentTypeAst);

                if (fragmentType != null && !DoesFragmentTypeApply(objectDefinition, fragmentType))
                    continue;

                var fragmentSelectionSet = inlineFragment.SelectionSet;
                var fragmentGroupedFieldSet = CollectFields(
                    schema,
                    document,
                    objectDefinition,
                    fragmentSelectionSet,
                    coercedVariableValues,
                    visitedFragments);

                foreach (var fragmentGroup in fragmentGroupedFieldSet)
                {
                    var responseKey = fragmentGroup.Key;

                    if (!groupedFields.ContainsKey(responseKey))
                        groupedFields[responseKey] = new List<FieldSelection>();

                    groupedFields[responseKey].AddRange(fragmentGroup.Value);
                }
            }
        }

        return groupedFields;
    }

    private static bool IncludeSelection(
        Directive? includeDirective,
        IReadOnlyDictionary<string, object?> coercedVariableValues,
        ISchema schema,
        ObjectDefinition objectDefinition,
        object selection)
    {
        if (includeDirective?.Arguments == null)
            return true;

        var ifArgument = includeDirective.Arguments.SingleOrDefault(a => a.Name == "if");
        return GetIfArgumentValue(schema, includeDirective, coercedVariableValues, ifArgument);
    }

    private static bool SkipSelection(
        Directive? skipDirective,
        IReadOnlyDictionary<string, object?> coercedVariableValues,
        ISchema schema,
        ObjectDefinition objectDefinition,
        object selection)
    {
        if (skipDirective?.Arguments == null)
            return false;

        var ifArgument = skipDirective.Arguments.SingleOrDefault(a => a.Name == "if");
        return GetIfArgumentValue(schema, skipDirective, coercedVariableValues, ifArgument);
    }

    private static bool GetIfArgumentValue(
        ISchema schema,
        Directive directive,
        IReadOnlyDictionary<string, object?> coercedVariableValues,
        Argument? argument)
    {
        if (argument is null)
            return false;

        return Ast.GetIfArgumentValue(directive, coercedVariableValues, argument);
    }

    private static IEnumerable<Directive> GetDirectives(ISelection selection)
    {
        return selection.Directives ?? Language.Nodes.Directives.None;
    }

    private static bool DoesFragmentTypeApply(ObjectDefinition objectDefinition, TypeDefinition fragmentType)
    {
        return Ast.DoesFragmentTypeApply(objectDefinition, fragmentType);
    }
}
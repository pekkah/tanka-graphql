using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Core2
{
    public class FieldCollector
    {
        private readonly CoerceValue _coerceValue;

        public FieldCollector(
            CoerceValue coerceValue)
        {
            _coerceValue = coerceValue;
        }

        public Dictionary<string, List<FieldSelection>> CollectFields(
            OperationContext context,
            ObjectDefinition objectDefinition,
            SelectionSet selectionSet,
            List<string>? visitedFragments = null,
            CancellationToken cancellationToken = default)
        {
            visitedFragments ??= new List<string>();
            var schema = context.Schema;
            var document = context.Document;
            var coercedVariableValues = context.CoercedVariableValues;
            var fragments = document.FragmentDefinitions
                            ?? FragmentDefinitions.None;

            var groupedFields = new Dictionary<string, List<FieldSelection>>();
            foreach (var selection in selectionSet)
            {
                var directives = selection.Directives ?? Language.Nodes.Directives.None;

                var skipDirective = directives.FirstOrDefault(d => d.Name == "skip");
                if (skipDirective != null && SkipSelection(skipDirective, coercedVariableValues, schema, _coerceValue))
                    continue;

                var includeDirective = directives.FirstOrDefault(d => d.Name == "include");
                if (includeDirective != null &&
                    !IncludeSelection(includeDirective, coercedVariableValues, schema, _coerceValue))
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

                    var fragment = fragments
                        .SingleOrDefault(f => f.FragmentName == fragmentSpreadName);

                    if (fragment == null)
                        continue;

                    var fragmentTypeAst = fragment.TypeCondition;
                    var fragmentType = Ast.TypeFromAst(schema, fragmentTypeAst);

                    if (fragmentType == null || !Ast.DoesFragmentTypeApply(objectDefinition, fragmentType))
                        continue;

                    var fragmentSelectionSet = fragment.SelectionSet;
                    var fragmentGroupedFieldSet = CollectFields(
                        context,
                        objectDefinition,
                        fragmentSelectionSet,
                        visitedFragments,
                        cancellationToken); //todo: nested defer?


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
                    var fragmentType = Ast.TypeFromAst(schema, fragmentTypeAst);

                    if (fragmentType != null && !Ast.DoesFragmentTypeApply(objectDefinition, fragmentType))
                        continue;

                    var fragmentSelectionSet = inlineFragment.SelectionSet;
                    var fragmentGroupedFieldSet = CollectFields(
                        context,
                        objectDefinition,
                        fragmentSelectionSet,
                        visitedFragments,
                        cancellationToken); //todo: nested defer?

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

        private bool IncludeSelection(
            Directive includeDirective,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            ExecutableSchema schema,
            CoerceValue coerceValue)
        {
            var ifArgument = includeDirective
                .Arguments
                ?.SingleOrDefault(a => a.Name == "if");

            if (ifArgument == null)
                return true;

            return Ast.GetIfArgumentValue(
                schema,
                includeDirective,
                coercedVariableValues,
                ifArgument,
                coerceValue);
        }

        private bool SkipSelection(
            Directive skipDirective,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            ExecutableSchema schema,
            CoerceValue coerceValue)
        {
            var ifArgument = skipDirective
                .Arguments
                ?.SingleOrDefault(a => a.Name == "if");

            if (ifArgument == null)
                return false;

            return Ast.GetIfArgumentValue(
                schema,
                skipDirective,
                coercedVariableValues,
                ifArgument,
                coerceValue);
        }
    }
}
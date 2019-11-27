using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;
using static Tanka.GraphQL.TypeSystem.Ast;

namespace Tanka.GraphQL.Execution
{
    public static class SelectionSets
    {
        public static async Task<IDictionary<string, object>> ExecuteSelectionSetAsync(
            IExecutorContext executorContext,
            GraphQLSelectionSet selectionSet,
            ObjectType objectType,
            object objectValue,
            NodePath path)
        {
            if (executorContext == null) throw new ArgumentNullException(nameof(executorContext));
            if (selectionSet == null) throw new ArgumentNullException(nameof(selectionSet));
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var groupedFieldSet = CollectFields(
                executorContext.Schema,
                executorContext.Document,
                objectType,
                selectionSet,
                executorContext.CoercedVariableValues);

            var resultMap = await executorContext.Strategy.ExecuteGroupedFieldSetAsync(
                executorContext,
                groupedFieldSet,
                objectType,
                objectValue,
                path).ConfigureAwait(false);

            return resultMap;
        }


        public static GraphQLSelectionSet MergeSelectionSets(IReadOnlyCollection<GraphQLFieldSelection> fields)
        {
            var selectionSet = new List<ASTNode>();
            foreach (var field in fields)
            {
                var fieldSelectionSet = field.SelectionSet;
                if (fieldSelectionSet?.Selections == null || !fieldSelectionSet.Selections.Any()) continue;

                selectionSet.AddRange(fieldSelectionSet.Selections);
            }

            return new GraphQLSelectionSet
            {
                Selections = selectionSet
            };
        }

        public static IReadOnlyDictionary<string, List<GraphQLFieldSelection>> CollectFields(
            ISchema schema,
            GraphQLDocument document,
            ObjectType objectType,
            GraphQLSelectionSet selectionSet,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            List<string> visitedFragments = null)
        {
            if (visitedFragments == null)
                visitedFragments = new List<string>();

            var fragments = document.Definitions
                .OfType<GraphQLFragmentDefinition>()
                .ToList();

            var groupedFields = new Dictionary<string, List<GraphQLFieldSelection>>();
            foreach (var selection in selectionSet.Selections)
            {
                var directives = GetDirectives(selection).ToList();

                var skipDirective = directives.FirstOrDefault(d => d.Name.Value == "skip");
                if (SkipSelection(skipDirective, coercedVariableValues, schema, objectType, selection))
                    continue;

                var includeDirective = directives.FirstOrDefault(d => d.Name.Value == "include");
                if (!IncludeSelection(includeDirective, coercedVariableValues, schema, objectType, selection))
                    continue;

                if (selection is GraphQLFieldSelection fieldSelection)
                {
                    var name = fieldSelection.Alias?.Value ?? fieldSelection.Name.Value;
                    if (!groupedFields.ContainsKey(name))
                        groupedFields[name] = new List<GraphQLFieldSelection>();

                    groupedFields[name].Add(fieldSelection);
                }

                if (selection is GraphQLFragmentSpread fragmentSpread)
                {
                    var fragmentSpreadName = fragmentSpread.Name.Value;

                    if (visitedFragments.Contains(fragmentSpreadName)) continue;

                    visitedFragments.Add(fragmentSpreadName);

                    var fragment = fragments.SingleOrDefault(f => f.Name.Value == fragmentSpreadName);

                    if (fragment == null)
                        continue;

                    var fragmentTypeAst = fragment.TypeCondition;
                    var fragmentType = TypeFromAst(schema, fragmentTypeAst);

                    if (!DoesFragmentTypeApply(objectType, fragmentType))
                        continue;

                    var fragmentSelectionSet = fragment.SelectionSet;
                    var fragmentGroupedFieldSet = CollectFields(
                        schema,
                        document,
                        objectType,
                        fragmentSelectionSet,
                        coercedVariableValues,
                        visitedFragments);

                    foreach (var fragmentGroup in fragmentGroupedFieldSet)
                    {
                        var responseKey = fragmentGroup.Key;

                        if (!groupedFields.ContainsKey(responseKey))
                            groupedFields[responseKey] = new List<GraphQLFieldSelection>();

                        groupedFields[responseKey].AddRange(fragmentGroup.Value);
                    }
                }

                if (selection is GraphQLInlineFragment inlineFragment)
                {
                    var fragmentTypeAst = inlineFragment.TypeCondition;
                    var fragmentType = TypeFromAst(schema, fragmentTypeAst);

                    if (fragmentType != null && !DoesFragmentTypeApply(objectType, fragmentType))
                        continue;

                    var fragmentSelectionSet = inlineFragment.SelectionSet;
                    var fragmentGroupedFieldSet = CollectFields(
                        schema,
                        document,
                        objectType,
                        fragmentSelectionSet,
                        coercedVariableValues,
                        visitedFragments);

                    foreach (var fragmentGroup in fragmentGroupedFieldSet)
                    {
                        var responseKey = fragmentGroup.Key;

                        if (!groupedFields.ContainsKey(responseKey))
                            groupedFields[responseKey] = new List<GraphQLFieldSelection>();

                        groupedFields[responseKey].AddRange(fragmentGroup.Value);
                    }
                }
            }

            return groupedFields;
        }

        private static bool IncludeSelection(GraphQLDirective includeDirective,
            IReadOnlyDictionary<string, object> coercedVariableValues, ISchema schema, ObjectType objectType, ASTNode selection)
        {
            if (includeDirective == null)
                return true;

            var ifArgument = includeDirective.Arguments.SingleOrDefault(a => a.Name?.Value == "if");
            return GetIfArgumentValue(schema, includeDirective, coercedVariableValues, ifArgument);
        }

        private static bool SkipSelection(GraphQLDirective skipDirective,
            IReadOnlyDictionary<string, object> coercedVariableValues, ISchema schema, ObjectType objectType, ASTNode selection)
        {
            if (skipDirective == null)
                return false;

            var ifArgument = skipDirective.Arguments.SingleOrDefault(a => a.Name?.Value == "if");
            return GetIfArgumentValue(schema, skipDirective, coercedVariableValues, ifArgument);
        }

        private static bool GetIfArgumentValue(
            ISchema schema,
            GraphQLDirective directive,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            GraphQLArgument argument)
        {
            if (directive == null) throw new ArgumentNullException(nameof(directive));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

            switch (argument.Value)
            {
                case GraphQLScalarValue scalarValue:
                    return (bool) Values.CoerceValue(schema.GetInputFields, scalarValue, ScalarType.NonNullBoolean);
                case GraphQLVariable variable:
                    var variableValue = coercedVariableValues[variable.Name.Value];
                    return (bool) variableValue;
                default:
                    return false;
            }
        }

        private static IEnumerable<GraphQLDirective> GetDirectives(ASTNode node)
        {
            switch (node)
            {
                case GraphQLFieldSelection fieldSelection:
                    return fieldSelection.Directives;
                case GraphQLFragmentSpread fragmentSpread:
                    return fragmentSpread.Directives;
                case GraphQLInlineFragment inlineFragment:
                    return inlineFragment.Directives;
                default:
                    return Enumerable.Empty<GraphQLDirective>();
            }
        }

        private static bool DoesFragmentTypeApply(ObjectType objectType, IType fragmentType)
        {
            if (fragmentType is ObjectType obj)
                return string.Equals(obj.Name, objectType.Name, StringComparison.Ordinal);

            if (fragmentType is InterfaceType interfaceType) return objectType.Implements(interfaceType);

            if (fragmentType is UnionType unionType) return unionType.IsPossible(objectType);

            return false;
        }
    }
}
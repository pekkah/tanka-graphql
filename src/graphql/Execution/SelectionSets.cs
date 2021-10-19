using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using static Tanka.GraphQL.TypeSystem.Ast;
using Argument = Tanka.GraphQL.TypeSystem.Argument;

namespace Tanka.GraphQL.Execution
{
    public static class SelectionSets
    {
        public static async Task<IDictionary<string, object>> ExecuteSelectionSetAsync(
            IExecutorContext executorContext,
            SelectionSet selectionSet,
            ObjectType objectType,
            object objectValue,
            NodePath path)
        {
            if (executorContext == null) throw new ArgumentNullException(nameof(executorContext));
            if (selectionSet == null) throw new ArgumentNullException(nameof(selectionSet));
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


        public static SelectionSet MergeSelectionSets(IReadOnlyCollection<FieldSelection> fields)
        {
            var selectionSet = new List<ISelection>();
            foreach (var field in fields)
            {
                var fieldSelectionSet = field.SelectionSet;
                if (fieldSelectionSet?.Selections == null || !fieldSelectionSet.Selections.Any()) continue;

                selectionSet.AddRange(fieldSelectionSet.Selections);
            }

            return new SelectionSet(selectionSet);
        }

        public static IReadOnlyDictionary<string, List<FieldSelection>> CollectFields(
            ISchema schema,
            ExecutableDocument document,
            ObjectType objectType,
            SelectionSet selectionSet,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            List<string>? visitedFragments = null)
        {
            visitedFragments ??= new List<string>();

            var fragments = document.FragmentDefinitions;

            var groupedFields = new Dictionary<string, List<FieldSelection>>();
            foreach (var selection in selectionSet.Selections)
            {
                var directives = GetDirectives(selection).ToList();

                var skipDirective = directives.FirstOrDefault(d => d.Name == "skip"); //todo: skip to constant
                if (SkipSelection(skipDirective, coercedVariableValues, schema, objectType, selection))
                    continue;

                var includeDirective = directives.FirstOrDefault(d => d.Name == "include"); //todo: include to constant
                if (!IncludeSelection(includeDirective, coercedVariableValues, schema, objectType, selection))
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
                            groupedFields[responseKey] = new List<FieldSelection>();

                        groupedFields[responseKey].AddRange(fragmentGroup.Value);
                    }
                }

                if (selection is InlineFragment inlineFragment)
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
                            groupedFields[responseKey] = new List<FieldSelection>();

                        groupedFields[responseKey].AddRange(fragmentGroup.Value);
                    }
                }
            }

            return groupedFields;
        }

        private static bool IncludeSelection(Directive includeDirective,
            IReadOnlyDictionary<string, object?> coercedVariableValues, ISchema schema, ObjectType objectType, object selection)
        {
            if (includeDirective?.Arguments == null)
                return true;

            var ifArgument = includeDirective.Arguments.SingleOrDefault(a => a.Name == "if"); //todo: if to constants
            return GetIfArgumentValue(schema, includeDirective, coercedVariableValues, ifArgument);
        }

        private static bool SkipSelection(Directive skipDirective,
            IReadOnlyDictionary<string, object?> coercedVariableValues, ISchema schema, ObjectType objectType, object selection)
        {
            if (skipDirective?.Arguments == null)
                return false;

            var ifArgument = skipDirective.Arguments.SingleOrDefault(a => a.Name == "if"); //todo: if to constants
            return GetIfArgumentValue(schema, skipDirective, coercedVariableValues, ifArgument);
        }

        private static bool GetIfArgumentValue(
            ISchema schema,
            Directive directive,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            Language.Nodes.Argument argument)
        {
            if (directive == null) throw new ArgumentNullException(nameof(directive));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

            switch (argument.Value.Kind)
            {
                case NodeKind.BooleanValue:
                    return (bool) Values.CoerceValue(schema.GetInputFields, schema.GetValueConverter, (BooleanValue)argument.Value, ScalarType.NonNullBoolean);
                case NodeKind.Variable:
                    var variable = (Variable) argument.Value;
                    var variableValue = coercedVariableValues[variable.Name];
                    
                    if (variableValue == null)
                        throw new QueryExecutionException(
                            $"If argument of {directive} is null. Variable value should not be null",
                            new NodePath(), argument);
                    
                    return (bool) variableValue;
                default:
                    return false;
            }
        }

        private static IEnumerable<Directive> GetDirectives(ISelection selection)
        {
            return selection.Directives ?? Language.Nodes.Directives.None;
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
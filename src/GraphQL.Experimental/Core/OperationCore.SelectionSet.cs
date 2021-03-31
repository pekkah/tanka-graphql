using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Core
{
    public partial class OperationCore
    {
        public static Task<SelectionSetResult> ExecuteSelectionSet(
            OperationContext context,
            ObjectDefinition objectDefinition,
            object? objectValue,
            SelectionSet selectionSet,
            NodePath path,
            CollectFields collectFields,
            ExecuteField executeField,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return context.Operation.Operation switch
            {
                OperationType.Query => ExecuteSelectionSetParallel(
                    context,
                    objectDefinition,
                    objectValue,
                    selectionSet,
                    path,
                    collectFields,
                    executeField,
                    cancellationToken),
                OperationType.Mutation => ExecuteSelectionSetSerial(
                    context,
                    objectDefinition,
                    objectValue,
                    selectionSet,
                    path,
                    collectFields,
                    executeField,
                    cancellationToken),
                OperationType.Subscription => ExecuteSelectionSetParallel(
                    context,
                    objectDefinition,
                    objectValue,
                    selectionSet,
                    path,
                    collectFields,
                    executeField,
                    cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static async Task<SelectionSetResult> ExecuteSelectionSetParallel(
            OperationContext context,
            ObjectDefinition objectDefinition,
            object? objectValue,
            SelectionSet selectionSet,
            NodePath path,
            CollectFields collectFields,
            ExecuteField executeField,
            CancellationToken cancellationToken)
        {
            var groupedFieldSet = collectFields(
                context,
                objectDefinition,
                selectionSet,
                cancellationToken: cancellationToken);

            var tasks = new Dictionary<string, Task<object?>>();

            foreach (var (responseKey, fields) in groupedFieldSet)
            {
                var fieldPath = path.Fork();
                var fieldName = fields.First().Name;
                var fieldType = context.Schema
                    .GetField(objectDefinition, fieldName)
                    ?.Type;

                fieldPath.Append(fieldName);

                if (fieldType != null)
                    tasks.Add(responseKey, executeField(
                        context,
                        objectDefinition,
                        objectValue,
                        fieldType,
                        fields,
                        fieldPath,
                        cancellationToken));
            }

            await Task.WhenAll(tasks.Values);

            return new SelectionSetResult
            {
                Data = tasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result)
            };
        }

        public static async Task<SelectionSetResult> ExecuteSelectionSetSerial(
            OperationContext context,
            ObjectDefinition objectDefinition,
            object? objectValue,
            SelectionSet selectionSet,
            NodePath path,
            CollectFields collectFields,
            ExecuteField executeField,
            CancellationToken cancellationToken)
        {
            var groupedFieldSet = collectFields(
                context,
                objectDefinition,
                selectionSet,
                cancellationToken: cancellationToken);

            var data = new Dictionary<string, object?>();

            foreach (var (responseKey, fields) in groupedFieldSet)
            {
                var fieldPath = path.Fork();
                var fieldName = fields.First().Name;
                var fieldType = context.Schema
                    .GetField(objectDefinition, fieldName)
                    ?.Type;

                fieldPath.Append(fieldName);

                if (fieldType != null)
                    data.Add(responseKey, await executeField(
                        context,
                        objectDefinition,
                        objectValue,
                        fieldType,
                        fields,
                        fieldPath,
                        cancellationToken));
            }

            return new SelectionSetResult
            {
                Data = data
            };
        }

        public static Dictionary<string, List<FieldSelection>> CollectFields(
            OperationContext context,
            ObjectDefinition objectDefinition,
            SelectionSet selectionSet,
            CoerceValue coerceValue,
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
                var directives = GetDirectives(selection).ToList();

                var skipDirective = directives.FirstOrDefault(d => d.Name == "skip");
                if (skipDirective != null && SkipSelection(skipDirective, coercedVariableValues, schema, coerceValue))
                    continue;

                var includeDirective = directives.FirstOrDefault(d => d.Name == "include");
                if (includeDirective != null &&
                    !IncludeSelection(includeDirective, coercedVariableValues, schema, coerceValue))
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

                    if (fragmentType == null || !DoesFragmentTypeApply(objectDefinition, fragmentType))
                        continue;

                    var fragmentSelectionSet = fragment.SelectionSet;
                    var fragmentGroupedFieldSet = CollectFields(
                        context,
                        objectDefinition,
                        fragmentSelectionSet,
                        coerceValue,
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

                    if (fragmentType != null && !DoesFragmentTypeApply(objectDefinition, fragmentType))
                        continue;

                    var fragmentSelectionSet = inlineFragment.SelectionSet;
                    var fragmentGroupedFieldSet = CollectFields(
                        context,
                        objectDefinition,
                        fragmentSelectionSet,
                        coerceValue,
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

        private static bool IncludeSelection(
            Directive includeDirective,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            ExecutableSchema schema,
            CoerceValue coerceValue)
        {
            if (includeDirective?.Arguments == null)
                return true;

            var ifArgument = includeDirective
                .Arguments
                ?.SingleOrDefault(a => a.Name == "if");

            if (ifArgument == null)
                return true;

            return GetIfArgumentValue(
                schema,
                includeDirective,
                coercedVariableValues,
                ifArgument,
                coerceValue);
        }

        private static bool SkipSelection(Directive skipDirective,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            ExecutableSchema schema,
            CoerceValue coerceValue)
        {
            if (skipDirective?.Arguments == null)
                return false;

            var ifArgument = skipDirective
                .Arguments
                ?.SingleOrDefault(a => a.Name == "if");

            if (ifArgument == null)
                return false;

            return GetIfArgumentValue(
                schema,
                skipDirective,
                coercedVariableValues,
                ifArgument,
                coerceValue);
        }

        private static bool GetIfArgumentValue(
            ExecutableSchema schema,
            Directive directive,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            Argument argument,
            CoerceValue coerceValue)
        {
            if (directive == null) throw new ArgumentNullException(nameof(directive));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

            switch (argument.Value.Kind)
            {
                case NodeKind.BooleanValue:
                    return (bool) (coerceValue(schema, argument.Value, "Boolean!") ?? false);
                case NodeKind.Variable:
                    var variable = (Variable) argument.Value;
                    var variableValue = coercedVariableValues[variable.Name];

                    if (variableValue == null)
                        throw new Exception(
                            $"If argument of {directive} is null. Variable value should not be null");

                    return (bool) variableValue;
                default:
                    return false;
            }
        }

        private static IEnumerable<Directive> GetDirectives(ISelection selection)
        {
            return selection.Directives ?? Language.Nodes.Directives.None;
        }

        private static bool DoesFragmentTypeApply(
            ObjectDefinition objectType,
            TypeDefinition fragmentType)
        {
            if (objectType.Name == fragmentType.Name)
                return true;

            if (fragmentType is InterfaceDefinition interfaceType)
                return objectType
                    .Interfaces
                    ?.Any(implementedInterface => implementedInterface.Name == interfaceType.Name) == true;

            if (fragmentType is UnionDefinition unionType)
                return unionType
                    .Members
                    ?.Any(member => member.Name == objectType.Name) == true;

            return false;
        }
    }
}
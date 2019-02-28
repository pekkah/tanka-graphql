using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.type;
using tanka.graphql.type.converters;

namespace tanka.graphql.validation
{
    public static class ExecutionRules
    {
        public static IEnumerable<CombineRule> All = new[]
        {
            R511ExecutableDefinitions(),
            R5211OperationNameUniqueness(),
            R5221LoneAnonymousOperation(),
            R5511FragmentNameUniqueness(),
            R5512FragmentSpreadTypeExistence(),
            R5513FragmentsOnCompositeTypes(),
            R5514FragmentsMustBeUsed(),
            R5522FragmentSpreadsMustNotFormCycles(),
            R5523FragmentSpreadIsPossible(),
            R5231SingleRootField(),
            R531FieldSelections(),
            R533LeafFieldSelections(),
            R541ArgumentNames(),
            R542ArgumentUniqueness(),
            R5421RequiredArguments(),
            R561ValuesOfCorrectType(),
            R562InputObjectFieldNames(),
            R563InputObjectFieldUniqueness(),
            R564InputObjectRequiredFields(),
            R57Directives(),
            R58Variables()
        };


        /// <summary>
        ///     Formal Specification
        ///     For each definition definition in the document.
        ///     definition must be OperationDefinition or FragmentDefinition (it must not be TypeSystemDefinition).
        /// </summary>
        public static CombineRule R511ExecutableDefinitions()
        {
            return (context, rule) =>
            {
                rule.EnterDocument += document =>
                {
                    foreach (var definition in document.Definitions)
                    {
                        var valid = definition.Kind == ASTNodeKind.OperationDefinition
                                    || definition.Kind == ASTNodeKind.FragmentDefinition;

                        if (!valid)
                            context.Error(
                                ValidationErrorCodes.R511ExecutableDefinitions,
                                "GraphQL execution will only consider the " +
                                "executable definitions Operation and Fragment. " +
                                "Type system definitions and extensions are not " +
                                "executable, and are not considered during execution.",
                                definition);
                    }
                };
            };
        }

        /// <summary>
        ///     Formal Specification
        ///     For each operation definition operation in the document.
        ///     Let operationName be the name of operation.
        ///     If operationName exists
        ///     Let operations be all operation definitions in the document named operationName.
        ///     operations must be a set of one.
        /// </summary>
        public static CombineRule R5211OperationNameUniqueness()
        {
            return (context, rule) =>
            {
                var known = new List<string>();
                rule.EnterOperationDefinition += definition =>
                {
                    var operationName = definition.Name?.Value;

                    if (string.IsNullOrWhiteSpace(operationName))
                        return;

                    if (known.Contains(operationName))
                        context.Error(ValidationErrorCodes.R5211OperationNameUniqueness,
                            "Each named operation definition must be unique within a " +
                            "document when referred to by its name.",
                            definition);

                    known.Add(operationName);
                };
            };
        }

        /// <summary>
        ///     Let operations be all operation definitions in the document.
        ///     Let anonymous be all anonymous operation definitions in the document.
        ///     If operations is a set of more than 1:
        ///     anonymous must be empty.
        /// </summary>
        public static CombineRule R5221LoneAnonymousOperation()
        {
            return (context, rule) =>
            {
                rule.EnterDocument += document =>
                {
                    var operations = document.Definitions
                        .OfType<GraphQLOperationDefinition>()
                        .ToList();

                    var anonymous = operations
                        .Count(op => string.IsNullOrEmpty(op.Name?.Value));

                    if (operations.Count() > 1)
                        if (anonymous > 0)
                            context.Error(
                                ValidationErrorCodes.R5221LoneAnonymousOperation,
                                "GraphQL allows a short‐hand form for defining " +
                                "query operations when only that one operation exists in " +
                                "the document.",
                                operations);
                };
            };
        }

        /// <summary>
        ///     For each subscription operation definition subscription in the document
        ///     Let subscriptionType be the root Subscription type in schema.
        ///     Let selectionSet be the top level selection set on subscription.
        ///     Let variableValues be the empty set.
        ///     Let groupedFieldSet be the result of CollectFields(subscriptionType, selectionSet, variableValues).
        ///     groupedFieldSet must have exactly one entry.
        /// </summary>
        public static CombineRule R5231SingleRootField()
        {
            return (context, rule) =>
            {
                rule.EnterDocument += document =>
                {
                    var subscriptions = document.Definitions
                        .OfType<GraphQLOperationDefinition>()
                        .Where(op => op.Operation == OperationType.Subscription)
                        .ToList();

                    if (!subscriptions.Any())
                        return;

                    var schema = context.Schema;
                    //todo(pekka): should this report error?
                    if (schema.Subscription == null)
                        return;

                    var subscriptionType = schema.Subscription;
                    foreach (var subscription in subscriptions)
                    {
                        var selectionSet = subscription.SelectionSet;
                        var variableValues = new Dictionary<string, object>();

                        var groupedFieldSet = SelectionSets.CollectFields(
                            schema,
                            context.Document,
                            subscriptionType,
                            selectionSet,
                            variableValues);

                        if (groupedFieldSet.Count != 1)
                            context.Error(
                                ValidationErrorCodes.R5231SingleRootField,
                                "Subscription operations must have exactly one root field.",
                                subscription);
                    }
                };
            };
        }

        /// <summary>
        ///     For each selection in the document.
        ///     Let fieldName be the target field of selection
        ///     fieldName must be defined on type in scope
        /// </summary>
        public static CombineRule R531FieldSelections()
        {
            return (context, rule) =>
            {
                rule.EnterFieldSelection += selection =>
                {
                    var fieldName = selection.Name.Value;

                    if (fieldName == "__typename")
                        return;

                    if (context.Tracker.GetFieldDef() == null)
                        context.Error(
                            ValidationErrorCodes.R531FieldSelections,
                            "The target field of a field selection must be defined " +
                            "on the scoped type of the selection set. There are no " +
                            "limitations on alias names.",
                            selection);
                };
            };
        }

        /// <summary>
        ///     For each selection in the document
        ///     Let selectionType be the result type of selection
        ///     If selectionType is a scalar or enum:
        ///     The subselection set of that selection must be empty
        ///     If selectionType is an interface, union, or object
        ///     The subselection set of that selection must NOT BE empty
        /// </summary>
        public static CombineRule R533LeafFieldSelections()
        {
            return (context, rule) =>
            {
                rule.EnterFieldSelection += selection =>
                {
                    var fieldName = selection.Name.Value;

                    if (fieldName == "__typename")
                        return;

                    var field = context.Tracker.GetFieldDef();

                    if (field != null)
                    {
                        var selectionType = field.Value.Field.Type.Unwrap();
                        var hasSubSelection = selection.SelectionSet?.Selections?.Any();

                        if (selectionType is ScalarType && hasSubSelection == true)
                            context.Error(
                                ValidationErrorCodes.R533LeafFieldSelections,
                                "Field selections on scalars or enums are never " +
                                "allowed, because they are the leaf nodes of any GraphQL query.",
                                selection);

                        if (selectionType is EnumType && hasSubSelection == true)
                            context.Error(
                                ValidationErrorCodes.R533LeafFieldSelections,
                                "Field selections on scalars or enums are never " +
                                "allowed, because they are the leaf nodes of any GraphQL query.",
                                selection);

                        if (selectionType is ObjectType && hasSubSelection == null)
                            context.Error(
                                ValidationErrorCodes.R533LeafFieldSelections,
                                "Leaf selections on objects, interfaces, and unions " +
                                "without subfields are disallowed.",
                                selection);

                        if (selectionType is InterfaceType && hasSubSelection == null)
                            context.Error(
                                ValidationErrorCodes.R533LeafFieldSelections,
                                "Leaf selections on objects, interfaces, and unions " +
                                "without subfields are disallowed.",
                                selection);

                        if (selectionType is UnionType && hasSubSelection == null)
                            context.Error(
                                ValidationErrorCodes.R533LeafFieldSelections,
                                "Leaf selections on objects, interfaces, and unions " +
                                "without subfields are disallowed.",
                                selection);
                    }
                };
            };
        }

        /// <summary>
        ///     For each argument in the document
        ///     Let argumentName be the Name of argument.
        ///     Let argumentDefinition be the argument definition provided by the parent field or definition named argumentName.
        ///     argumentDefinition must exist.
        /// </summary>
        public static CombineRule R541ArgumentNames()
        {
            return (context, rule) =>
            {
                rule.EnterArgument += argument =>
                {
                    if (context.Tracker.GetArgument() == null)
                        context.Error(
                            ValidationErrorCodes.R541ArgumentNames,
                            "Every argument provided to a field or directive " +
                            "must be defined in the set of possible arguments of that " +
                            "field or directive.",
                            argument);
                };
            };
        }

        /// <summary>
        ///     For each Field or Directive in the document.
        ///     Let arguments be the arguments provided by the Field or Directive.
        ///     Let argumentDefinitions be the set of argument definitions of that Field or Directive.
        ///     For each argumentDefinition in argumentDefinitions:
        ///     - Let type be the expected type of argumentDefinition.
        ///     - Let defaultValue be the default value of argumentDefinition.
        ///     - If type is Non‐Null and defaultValue does not exist:
        ///     - Let argumentName be the name of argumentDefinition.
        ///     - Let argument be the argument in arguments named argumentName
        ///     argument must exist.
        ///     - Let value be the value of argument.
        ///     value must not be the null literal.
        /// </summary>
        public static CombineRule R5421RequiredArguments()
        {
            IEnumerable<KeyValuePair<string, Argument>> GetArgumentDefinitions(IRuleVisitorContext context)
            {
                var definitions = context.Tracker.GetDirective()?.Arguments
                                  ?? context.Tracker.GetFieldDef()?.Field.Arguments;

                return definitions;
            }

            void ValidateArguments(IEnumerable<KeyValuePair<string, Argument>> keyValuePairs,
                List<GraphQLArgument> graphQLArguments, IRuleVisitorContext ruleVisitorContext)
            {
                foreach (var argumentDefinition in keyValuePairs)
                {
                    var type = argumentDefinition.Value.Type;
                    var defaultValue = argumentDefinition.Value.DefaultValue;

                    if (!(type is NonNull nonNull) || defaultValue != null)
                        continue;

                    var argumentName = argumentDefinition.Key;
                    var argument = graphQLArguments.SingleOrDefault(a => a.Name.Value == argumentName);

                    if (argument == null)
                    {
                        ruleVisitorContext.Error(
                            ValidationErrorCodes.R5421RequiredArguments,
                            "Arguments is required. An argument is required " +
                            "if the argument type is non‐null and does not have a default " +
                            "value. Otherwise, the argument is optional. " +
                            $"Argument {argumentName} not given",
                            graphQLArguments);

                        return;
                    }

                    // We don't want to throw error here due to non-null so we use the WrappedType directly
                    var argumentValue =
                        Values.CoerceValue(ruleVisitorContext.Schema, argument.Value, nonNull.WrappedType);
                    if (argumentValue == null)
                        ruleVisitorContext.Error(
                            ValidationErrorCodes.R5421RequiredArguments,
                            "Arguments is required. An argument is required " +
                            "if the argument type is non‐null and does not have a default " +
                            "value. Otherwise, the argument is optional. " +
                            $"Value of argument {argumentName} cannot be null",
                            graphQLArguments);
                }
            }

            return (context, rule) =>
            {
                rule.EnterFieldSelection += field =>
                {
                    var args = field.Arguments.ToList();
                    var argumentDefinitions = GetArgumentDefinitions(context);

                    //todo: should this produce error?
                    if (argumentDefinitions == null)
                        return;

                    ValidateArguments(argumentDefinitions, args, context);
                };
                rule.EnterDirective += directive =>
                {
                    var args = directive.Arguments.ToList();
                    var argumentDefinitions = GetArgumentDefinitions(context);

                    //todo: should this produce error?
                    if (argumentDefinitions == null)
                        return;

                    ValidateArguments(argumentDefinitions, args, context);
                };
            };
        }

        /// <summary>
        ///     For each argument in the Document.
        ///     Let argumentName be the Name of argument.
        ///     Let arguments be all Arguments named argumentName in the Argument Set which contains argument.
        ///     arguments must be the set containing only argument.
        /// </summary>
        public static CombineRule R542ArgumentUniqueness()
        {
            return (context, rule) =>
            {
                var knownArgs = new List<string>();
                rule.EnterFieldSelection += _ => knownArgs = new List<string>();
                rule.EnterDirective += _ => knownArgs = new List<string>();
                rule.EnterArgument += argument =>
                {
                    if (knownArgs.Contains(argument.Name.Value))
                        context.Error(
                            ValidationErrorCodes.R542ArgumentUniqueness,
                            "Fields and directives treat arguments as a mapping of " +
                            "argument name to value. More than one argument with the same " +
                            "name in an argument set is ambiguous and invalid.",
                            argument);

                    knownArgs.Add(argument.Name.Value);
                };
            };
        }

        /// <summary>
        ///     For each fragment definition fragment in the document
        ///     Let fragmentName be the name of fragment.
        ///     Let fragments be all fragment definitions in the document named fragmentName.
        ///     fragments must be a set of one.
        /// </summary>
        public static CombineRule R5511FragmentNameUniqueness()
        {
            return (context, rule) =>
            {
                var knownFragments = new List<string>();
                rule.EnterFragmentDefinition += fragment =>
                {
                    if (knownFragments.Contains(fragment.Name.Value))
                        context.Error(
                            ValidationErrorCodes.R5511FragmentNameUniqueness,
                            "Fragment definitions are referenced in fragment spreads by name. To avoid " +
                            "ambiguity, each fragment’s name must be unique within a document.",
                            fragment);

                    knownFragments.Add(fragment.Name.Value);
                };
            };
        }

        /// <summary>
        ///     For each named spread namedSpread in the document
        ///     Let fragment be the target of namedSpread
        ///     The target type of fragment must be defined in the schema
        /// </summary>
        public static CombineRule R5512FragmentSpreadTypeExistence()
        {
            return (context, rule) =>
            {
                rule.EnterFragmentDefinition += node =>
                {
                    var type = context.Tracker.GetCurrentType();

                    if (type == null)
                        context.Error(
                            ValidationErrorCodes.R5512FragmentSpreadTypeExistence,
                            "Fragments must be specified on types that exist in the schema. This " +
                            "applies for both named and inline fragments. ",
                            node);
                };
                rule.EnterInlineFragment += node =>
                {
                    var type = context.Tracker.GetCurrentType();

                    if (type == null)
                        context.Error(
                            ValidationErrorCodes.R5512FragmentSpreadTypeExistence,
                            "Fragments must be specified on types that exist in the schema. This " +
                            "applies for both named and inline fragments. ",
                            node);
                };
            };
        }

        /// <summary>
        ///     For each fragment defined in the document.
        ///     The target type of fragment must have kind UNION, INTERFACE, or OBJECT.
        /// </summary>
        public static CombineRule R5513FragmentsOnCompositeTypes()
        {
            return (context, rule) =>
            {
                rule.EnterFragmentDefinition += node =>
                {
                    var type = context.Tracker.GetCurrentType();

                    if (type is UnionType)
                        return;

                    if (type is ComplexType)
                        return;

                    context.Error(
                        ValidationErrorCodes.R5513FragmentsOnCompositeTypes,
                        "Fragments can only be declared on unions, interfaces, and objects",
                        node);
                };
                rule.EnterInlineFragment += node =>
                {
                    var type = context.Tracker.GetCurrentType();

                    if (type is UnionType)
                        return;

                    if (type is ComplexType)
                        return;

                    context.Error(
                        ValidationErrorCodes.R5513FragmentsOnCompositeTypes,
                        "Fragments can only be declared on unions, interfaces, and objects",
                        node);
                };
            };
        }

        /// <summary>
        ///     For each fragment defined in the document.
        ///     fragment must be the target of at least one spread in the document
        /// </summary>
        public static CombineRule R5514FragmentsMustBeUsed()
        {
            return (context, rule) =>
            {
                var fragments = new Dictionary<string, GraphQLFragmentDefinition>();
                var fragmentSpreads = new List<string>();

                rule.EnterFragmentDefinition += fragment => { fragments.Add(fragment.Name.Value, fragment); };
                rule.EnterFragmentSpread += spread => { fragmentSpreads.Add(spread.Name.Value); };
                rule.LeaveDocument += document =>
                {
                    foreach (var fragment in fragments)
                    {
                        var name = fragment.Key;
                        if (!fragmentSpreads.Contains(name))
                            context.Error(
                                ValidationErrorCodes.R5514FragmentsMustBeUsed,
                                "Defined fragments must be used within a document.",
                                fragment.Value);
                    }
                };
            };
        }

        public static CombineRule R5522FragmentSpreadsMustNotFormCycles()
        {
            return (context, rule) =>
            {
                var visitedFrags = new List<string>();
                var spreadPath = new Stack<GraphQLFragmentSpread>();

                // Position in the spread path
                var spreadPathIndexByName = new Dictionary<string, int?>();

                var fragments = context.Document.Definitions.OfType<GraphQLFragmentDefinition>()
                    .ToList();

                rule.EnterFragmentDefinition += node =>
                {
                    DetectCycleRecursive(
                        node,
                        spreadPath,
                        visitedFrags,
                        spreadPathIndexByName,
                        context,
                        fragments);
                };
            };

            string CycleErrorMessage(string fragName, string[] spreadNames)
            {
                var via = spreadNames.Any() ? " via " + string.Join(", ", spreadNames) : "";
                return "The graph of fragment spreads must not form any cycles including spreading itself. " +
                       "Otherwise an operation could infinitely spread or infinitely execute on cycles in the " +
                       "underlying data. " +
                       $"Cannot spread fragment \"{fragName}\" within itself {via}.";
            }

            IEnumerable<GraphQLFragmentSpread> GetFragmentSpreads(GraphQLSelectionSet node)
            {
                var spreads = new List<GraphQLFragmentSpread>();

                var setsToVisit = new Stack<GraphQLSelectionSet>(new[] {node});

                while (setsToVisit.Any())
                {
                    var set = setsToVisit.Pop();

                    foreach (var selection in set.Selections)
                        if (selection is GraphQLFragmentSpread spread)
                            spreads.Add(spread);
                        else if (selection is GraphQLFieldSelection fieldSelection)
                            if (fieldSelection.SelectionSet != null)
                                setsToVisit.Push(fieldSelection.SelectionSet);
                }

                return spreads;
            }

            void DetectCycleRecursive(
                GraphQLFragmentDefinition fragment,
                Stack<GraphQLFragmentSpread> spreadPath,
                List<string> visitedFrags,
                Dictionary<string, int?> spreadPathIndexByName,
                IRuleVisitorContext context,
                List<GraphQLFragmentDefinition> fragments)
            {
                var fragmentName = fragment.Name.Value;
                if (visitedFrags.Contains(fragmentName))
                    return;

                var spreadNodes = GetFragmentSpreads(fragment.SelectionSet)
                    .ToArray();

                if (!spreadNodes.Any())
                    return;

                spreadPathIndexByName[fragmentName] = spreadPath.Count;

                for (var i = 0; i < spreadNodes.Length; i++)
                {
                    var spreadNode = spreadNodes[i];
                    var spreadName = spreadNode.Name.Value;
                    var cycleIndex = spreadPathIndexByName.ContainsKey(spreadName)
                        ? spreadPathIndexByName[spreadName]
                        : default;

                    spreadPath.Push(spreadNode);

                    if (cycleIndex == null)
                    {
                        var spreadFragment = fragments.SingleOrDefault(f => f.Name.Value == spreadName);

                        if (spreadFragment != null)
                            DetectCycleRecursive(
                                spreadFragment,
                                spreadPath,
                                visitedFrags,
                                spreadPathIndexByName,
                                context,
                                fragments);
                    }
                    else
                    {
                        var cyclePath = spreadPath.Skip(cycleIndex.Value).ToList();
                        var fragmentNames = cyclePath.Take(cyclePath.Count() - 1)
                            .Select(s => s.Name.Value)
                            .ToArray();

                        context.Error(
                            ValidationErrorCodes.R5522FragmentSpreadsMustNotFormCycles,
                            CycleErrorMessage(spreadName, fragmentNames),
                            cyclePath);
                    }

                    spreadPath.Pop();
                }

                spreadPathIndexByName[fragmentName] = null;
            }
        }

        /// <summary>
        ///     For each spread (named or inline) defined in the document.
        ///     Let fragment be the target of spread
        ///     Let fragmentType be the type condition of fragment
        ///     Let parentType be the type of the selection set containing spread
        ///     Let applicableTypes be the intersection of GetPossibleTypes(fragmentType) and GetPossibleTypes(parentType)
        ///     applicableTypes must not be empty.
        /// </summary>
        /// <returns></returns>
        public static CombineRule R5523FragmentSpreadIsPossible()
        {
            return (context, rule) =>
            {
                var fragments = context.Document.Definitions.OfType<GraphQLFragmentDefinition>()
                    .ToDictionary(f => f.Name.Value);

                rule.EnterFragmentSpread += node =>
                {
                    var fragment = fragments[node.Name.Value];
                    var fragmentType = Ast.TypeFromAst(context.Schema, fragment.TypeCondition);
                    var parentType = context.Tracker.GetParentType();
                    var applicableTypes = GetPossibleTypes(fragmentType, context.Schema)
                        .Intersect(GetPossibleTypes(parentType, context.Schema));

                    if (!applicableTypes.Any())
                        context.Error(
                            ValidationErrorCodes.R5523FragmentSpreadIsPossible,
                            "Fragments are declared on a type and will only apply " +
                            "when the runtime object type matches the type condition. They " +
                            "also are spread within the context of a parent type. A fragment " +
                            "spread is only valid if its type condition could ever apply within " +
                            "the parent type.",
                            node);
                };

                rule.EnterInlineFragment += node =>
                {
                    var fragmentType = Ast.TypeFromAst(context.Schema, node.TypeCondition);
                    var parentType = context.Tracker.GetParentType();
                    var applicableTypes = GetPossibleTypes(fragmentType, context.Schema)
                        .Intersect(GetPossibleTypes(parentType, context.Schema));

                    if (!applicableTypes.Any())
                        context.Error(
                            ValidationErrorCodes.R5523FragmentSpreadIsPossible,
                            "Fragments are declared on a type and will only apply " +
                            "when the runtime object type matches the type condition. They " +
                            "also are spread within the context of a parent type. A fragment " +
                            "spread is only valid if its type condition could ever apply within " +
                            "the parent type.",
                            node);
                };
            };

            ObjectType[] GetPossibleTypes(IType type, ISchema schema)
            {
                switch (type)
                {
                    case ObjectType objectType:
                        return new[] {objectType};
                    case InterfaceType interfaceType:
                        return schema.GetPossibleTypes(interfaceType).ToArray();
                    case UnionType unionType:
                        return schema.GetPossibleTypes(unionType).ToArray();
                    default: return new ObjectType[] { };
                }
            }
        }

        public static CombineRule R561ValuesOfCorrectType()
        {
            return (context, rule) =>
            {
                //todo: there's an astnodekind for nullvalue but no type
                //rule.EnterNullValue += node => { };

                rule.EnterListValue += node =>
                {
                    var type = context.Tracker.GetNullableType(
                        context.Tracker.GetParentInputType());

                    if (!(type is List)) IsValidScalar(context, node);
                };
                rule.EnterObjectValue += node =>
                {
                    var type = context.Tracker.GetNamedType(
                        context.Tracker.GetInputType());

                    if (!(type is InputObjectType inputType))
                    {
                        IsValidScalar(context, node);
                        // return false;
                        return;
                    }

                    var fieldNodeMap = node.Fields.ToDictionary(
                        f => f.Name.Value);

                    foreach (var fieldDef in context.Schema.GetInputFields(
                        inputType.Name))
                    {
                        var fieldNode = fieldNodeMap.ContainsKey(fieldDef.Key);
                        if (!fieldNode && fieldDef.Value.Type is NonNull nonNull)
                            context.Error(
                                ValidationErrorCodes.R561ValuesOfCorrectType,
                                RequiredFieldMessage(
                                    type.ToString(),
                                    fieldDef.Key,
                                    nonNull.ToString()),
                                node);
                    }
                };
                rule.EnterObjectField += node =>
                {
                    var parentType = context.Tracker
                        .GetNamedType(context.Tracker.GetParentInputType());

                    var fieldType = context.Tracker.GetInputType();
                    if (fieldType == null && parentType is InputObjectType)
                        context.Error(
                            ValidationErrorCodes.R561ValuesOfCorrectType,
                            UnknownFieldMessage(
                                parentType.ToString(),
                                node.Name.Value,
                                string.Empty),
                            node);
                };
                rule.EnterEnumValue += node =>
                {
                    var maybeEnumType = context.Tracker.GetNamedType(
                        context.Tracker.GetInputType());

                    if (!(maybeEnumType is EnumType type))
                        IsValidScalar(context, node);
                    else if (type.ParseValue(node.Value) == null)
                        context.Error(
                            ValidationErrorCodes.R561ValuesOfCorrectType,
                            BadValueMessage(
                                type.Name,
                                node.ToString(),
                                string.Empty));
                };
                rule.EnterIntValue += node => IsValidScalar(context, node);
                rule.EnterFloatValue += node => IsValidScalar(context, node);
                rule.EnterStringValue += node => IsValidScalar(context, node);
                rule.EnterBooleanValue += node => IsValidScalar(context, node);
            };

            string BadValueMessage(
                string typeName,
                string valueName,
                string message
            )
            {
                return $"Expected type {typeName}, found {valueName} " +
                       message;
            }

            string RequiredFieldMessage(
                string typeName,
                string fieldName,
                string fieldTypeName
            )
            {
                return $"Field {typeName}.{fieldName} of required type " +
                       $"{fieldTypeName} was not provided.";
            }

            string UnknownFieldMessage(
                string typeName,
                string fieldName,
                string message
            )
            {
                return $"Field {fieldName} is not defined by type {typeName} " +
                       message;
            }

            void IsValidScalar(
                IRuleVisitorContext context,
                GraphQLValue node)
            {
                var locationType = context.Tracker.GetInputType();

                if (locationType == null)
                    return;

                var maybeScalarType = context
                    .Tracker
                    .GetNamedType(locationType);

                if (!(maybeScalarType is IValueConverter type))
                {
                    context.Error(
                        ValidationErrorCodes.R561ValuesOfCorrectType,
                        BadValueMessage(
                            maybeScalarType?.ToString(),
                            node.ToString(),
                            string.Empty),
                        node);

                    return;
                }

                try
                {
                    type.ParseLiteral((GraphQLScalarValue) node);
                }
                catch (Exception e)
                {
                    context.Error(
                        ValidationErrorCodes.R561ValuesOfCorrectType,
                        BadValueMessage(locationType?.ToString(),
                            node.ToString(),
                            e.ToString()),
                        node);
                }
            }
        }

        public static CombineRule R562InputObjectFieldNames()
        {
            return (context, rule) =>
            {
                rule.EnterObjectField += inputField =>
                {
                    var inputFieldName = inputField.Name.Value;

                    if (!(context.Tracker
                        .GetParentInputType() is InputObjectType parentType))
                        return;

                    var inputFieldDefinition = context.Schema
                        .GetInputField(parentType.Name, inputFieldName);

                    if (inputFieldDefinition == null)
                        context.Error(
                            ValidationErrorCodes.R562InputObjectFieldNames,
                            "Every input field provided in an input object " +
                            "value must be defined in the set of possible fields of " +
                            "that input object’s expected type.",
                            inputField);
                };
            };
        }

        public static CombineRule R563InputObjectFieldUniqueness()
        {
            return (context, rule) =>
            {
                rule.EnterObjectValue += node =>
                {
                    var fields = node.Fields.ToList();

                    foreach (var inputField in fields)
                    {
                        var name = inputField.Name.Value;
                        if (fields.Count(f => f.Name.Value == name) > 1)
                            context.Error(
                                ValidationErrorCodes.R563InputObjectFieldUniqueness,
                                "Input objects must not contain more than one field " +
                                "of the same name, otherwise an ambiguity would exist which " +
                                "includes an ignored portion of syntax.",
                                fields.Where(f => f.Name.Value == name));
                    }
                };
            };
        }

        public static CombineRule R564InputObjectRequiredFields()
        {
            return (context, rule) =>
            {
                rule.EnterObjectValue += node =>
                {
                    var inputObject = context.Tracker.GetInputType() as InputObjectType;

                    if (inputObject == null)
                        return;

                    var fields = node.Fields.ToDictionary(f => f.Name.Value);
                    var fieldDefinitions = context.Schema.GetInputFields(inputObject.Name);

                    foreach (var fieldDefinition in fieldDefinitions)
                    {
                        var type = fieldDefinition.Value.Type;
                        var defaultValue = fieldDefinition.Value.DefaultValue;

                        if (type is NonNull nonNull && defaultValue == null)
                        {
                            var fieldName = fieldDefinition.Key;
                            if (!fields.TryGetValue(fieldName, out var field))
                            {
                                context.Error(
                                    ValidationErrorCodes.R564InputObjectRequiredFields,
                                    "Input object fields may be required. Much like a field " +
                                    "may have required arguments, an input object may have required " +
                                    "fields. An input field is required if it has a non‐null type and " +
                                    "does not have a default value. Otherwise, the input object field " +
                                    "is optional. " +
                                    $"Field '{nonNull}.{fieldName}' is required.",
                                    node);

                                return;
                            }

                            if (field.Value.Kind == ASTNodeKind.NullValue)
                                context.Error(
                                    ValidationErrorCodes.R564InputObjectRequiredFields,
                                    "Input object fields may be required. Much like a field " +
                                    "may have required arguments, an input object may have required " +
                                    "fields. An input field is required if it has a non‐null type and " +
                                    "does not have a default value. Otherwise, the input object field " +
                                    "is optional. " +
                                    $"Field '{nonNull}.{field}' value cannot be null.",
                                    node, field);
                        }
                    }
                };
            };
        }

        /// <summary>
        ///     5.7.1, 5.73
        /// </summary>
        /// <returns></returns>
        public static CombineRule R57Directives()
        {
            return (context, rule) =>
            {
                rule.EnterDirective += directive =>
                {
                    var directiveName = directive.Name.Value;
                    var directiveDefinition = context.Schema.GetDirective(directiveName);

                    if (directiveDefinition == null)
                        context.Error(
                            ValidationErrorCodes.R57Directives,
                            "GraphQL servers define what directives they support. " +
                            "For each usage of a directive, the directive must be available " +
                            "on that server.",
                            directive);
                };

                rule.EnterOperationDefinition += node => CheckDirectives(context, node.Directives);
                rule.EnterFieldSelection += node => CheckDirectives(context, node.Directives);
                rule.EnterFragmentDefinition += node => CheckDirectives(context, node.Directives);
                rule.EnterFragmentSpread += node => CheckDirectives(context, node.Directives);
                rule.EnterInlineFragment += node => CheckDirectives(context, node.Directives);
            };

            // 5.7.3
            void CheckDirectives(IRuleVisitorContext context, IEnumerable<GraphQLDirective> directives)
            {
                var knownDirectives = new List<string>();

                foreach (var directive in directives)
                {
                    if (knownDirectives.Contains(directive.Name.Value))
                        context.Error(
                            ValidationErrorCodes.R57Directives,
                            "For each usage of a directive, the directive must be used in a " +
                            "location that the server has declared support for. " +
                            $"Directive '{directive.Name.Value}' is used multiple times on same location",
                            directive);

                    knownDirectives.Add(directive.Name.Value);
                }
            }
        }

        /// <summary>
        ///     5.8.1, 5.8.2
        /// </summary>
        /// <returns></returns>
        public static CombineRule R58Variables()
        {
            return (context, rule) =>
            {
                rule.EnterOperationDefinition += node =>
                {
                    var knownVariables = new List<string>();
                    if (node.VariableDefinitions == null)
                        return;

                    foreach (var variableUsage in node.VariableDefinitions)
                    {
                        var variable = variableUsage.Variable;
                        var variableName = variable.Name.Value;

                        // 5.8.1 Variable Uniqueness
                        if (knownVariables.Contains(variableName))
                            context.Error(
                                ValidationErrorCodes.R58Variables,
                                "If any operation defines more than one " +
                                "variable with the same name, it is ambiguous and " +
                                "invalid. It is invalid even if the type of the " +
                                "duplicate variable is the same.",
                                node);

                        knownVariables.Add(variableName);

                        // 5.8.2
                        var variableType = Ast.TypeFromAst(context.Schema, variableUsage.Type);
                        if (!TypeIs.IsInputType(variableType))
                            context.Error(
                                ValidationErrorCodes.R58Variables,
                                "Variables can only be input types. Objects, unions, " +
                                "and interfaces cannot be used as inputs.." +
                                $"Given variable type is '{variableType}'",
                                node);
                    }
                };
            };
        }
    }
}
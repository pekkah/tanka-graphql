using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.Validation;

public static class ExecutionRules
{
    public static IEnumerable<CombineRule> All = new[]
    {
        /* R511ExecutableDefinitions(),*/

        R5211OperationNameUniqueness(),
        R5221LoneAnonymousOperation(),
        R5231SingleRootField(),

        R531FieldSelections(),
        R532FieldSelectionMerging(),
        R533LeafFieldSelections(),

        R541ArgumentNames(),
        R542ArgumentUniqueness(),
        R5421RequiredArguments(),

        R5511FragmentNameUniqueness(),
        R5512FragmentSpreadTypeExistence(),
        R5513FragmentsOnCompositeTypes(),
        R5514FragmentsMustBeUsed(),

        R5521FragmentSpreadTargetDefined(),
        R5522FragmentSpreadsMustNotFormCycles(),
        R5523FragmentSpreadIsPossible(),

        R561ValuesOfCorrectType(),
        R562InputObjectFieldNames(),
        R563InputObjectFieldUniqueness(),
        R564InputObjectRequiredFields(),

        R571And573Directives(),
        R572DirectivesAreInValidLocations(),


        R581And582Variables(),
        R583AllVariableUsesDefined(),
        R584AllVariablesUsed(),
        R585AllVariableUsagesAreAllowed()
    };


    /// <summary>
    ///     Formal Specification
    ///     For each definition definition in the document.
    ///     definition must be OperationDefinition or FragmentDefinition (it must not be TypeSystemDefinition).
    /// </summary>
    /* Not required as executable document can only contain operations or fragments or both
    public static CombineRule R511ExecutableDefinitions()
    {
        return (context, rule) =>
        {
            rule.EnterDocument += document =>
            {
                foreach (var definition in document.Definitions)
                {
                    var valid = definition.Kind == NodeKind.OperationDefinition
                                || definition.Kind == NodeKind.FragmentDefinition;

                    if (!valid)
                        context.Error(
                            ValidationErrorCodes.R511ExecutableDefinitions,
                            "GraphQL execution will only consider the " +
                            "executable definitions Operation and Fragment. " +
                            "Type system definitions and extensions are not " +
                            "executable, and are not considered during execution. " +
                            $"Non executable definition kind: '{definition.Kind}.'",
                            definition);
                }
            };
        };
    }
    */

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
                var operationName = definition.Name;

                if (string.IsNullOrWhiteSpace(operationName))
                    return;

                if (known.Contains(operationName))
                    context.Error(ValidationErrorCodes.R5211OperationNameUniqueness,
                        "Each named operation definition must be unique within a " +
                        "document when referred to by its name. " +
                        $"Operation: '{operationName}'",
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
                var operations = document.OperationDefinitions;

                if (operations == null)
                    return;

                var anonymous = operations
                    .Count(op => string.IsNullOrEmpty(op.Name));

                if (operations.Count() > 1)
                    if (anonymous > 0)
                        context.Error(
                            ValidationErrorCodes.R5221LoneAnonymousOperation,
                            "GraphQL allows a short‐hand form for defining " +
                            "query operations when only that one operation exists in " +
                            "the document.",
                            (INode)operations);
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
                var subscriptions = document
                    ?.OperationDefinitions
                    ?.Where(op => op.Operation == OperationType.Subscription)
                    .ToList();


                if (subscriptions == null || !subscriptions.Any())
                    return;

                var schema = context.Schema;
                if (schema.Subscription == null)
                    return;

                var subscriptionType = schema.Subscription;
                foreach (var subscription in subscriptions)
                {
                    var selectionSet = subscription.SelectionSet;
                    var variableValues = new Dictionary<string, object>();

                    var groupedFieldSet = SelectionSetExtensions.CollectFields(
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
                var fieldName = selection.Name;

                if (fieldName == "__typename")
                    return;

                if (context.Tracker.FieldDefinition == null)
                    context.Error(
                        ValidationErrorCodes.R531FieldSelections,
                        "The target field of a field selection must be defined " +
                        "on the scoped type of the selection set. There are no " +
                        "limitations on alias names. " +
                        $"Field: '{fieldName}'",
                        selection);
            };
        };
    }

    /// <summary>
    ///     If multiple field selections with the same response names are
    ///     encountered during execution, the field and arguments to execute and
    ///     the resulting value should be unambiguous. Therefore any two field
    ///     selections which might both be encountered for the same object are
    ///     only valid if they are equivalent.
    /// </summary>
    /// <returns></returns>
    public static CombineRule R532FieldSelectionMerging()
    {
        return (context, rule) =>
        {
            var validator = new FieldSelectionMergingValidator(context);
            rule.EnterSelectionSet += selectionSet => { validator.Validate(selectionSet); };
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
                var fieldName = selection.Name;

                if (fieldName == "__typename")
                    return;

                var field = context.Tracker.FieldDefinition;

                if (field != null)
                {
                    var selectionType = Ast.UnwrapAndResolveType(context.Schema, field.Type);
                    var hasSubSelection = selection.SelectionSet?.Any();

                    if (selectionType is ScalarDefinition && hasSubSelection == true)
                        context.Error(
                            ValidationErrorCodes.R533LeafFieldSelections,
                            "Field selections on scalars or enums are never " +
                            "allowed, because they are the leaf nodes of any GraphQL query. " +
                            $"Field: '{fieldName}'",
                            selection);

                    if (selectionType is EnumDefinition && hasSubSelection == true)
                        context.Error(
                            ValidationErrorCodes.R533LeafFieldSelections,
                            "Field selections on scalars or enums are never " +
                            "allowed, because they are the leaf nodes of any GraphQL query. " +
                            $"Field: '{fieldName}'",
                            selection);

                    if (selectionType is ObjectDefinition && hasSubSelection == null)
                        context.Error(
                            ValidationErrorCodes.R533LeafFieldSelections,
                            "Leaf selections on objects, interfaces, and unions " +
                            "without subfields are disallowed. " +
                            $"Field: '{fieldName}'",
                            selection);

                    if (selectionType is InterfaceDefinition && hasSubSelection == null)
                        context.Error(
                            ValidationErrorCodes.R533LeafFieldSelections,
                            "Leaf selections on objects, interfaces, and unions " +
                            "without subfields are disallowed. " +
                            $"Field: '{fieldName}'",
                            selection);

                    if (selectionType is UnionDefinition && hasSubSelection == null)
                        context.Error(
                            ValidationErrorCodes.R533LeafFieldSelections,
                            "Leaf selections on objects, interfaces, and unions " +
                            "without subfields are disallowed. " +
                            $"Field: '{fieldName}'",
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
                if (context.Tracker.ArgumentDefinition == null)
                    context.Error(
                        ValidationErrorCodes.R541ArgumentNames,
                        "Every argument provided to a field or directive " +
                        "must be defined in the set of possible arguments of that " +
                        "field or directive. " +
                        $"Argument: '{argument.Name.Value}'",
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
        ArgumentsDefinition GetFieldArgumentDefinitions(IRuleVisitorContext context)
        {
            var definitions = context
                .Tracker
                .FieldDefinition
                ?.Arguments;

            if (definitions == null)
                return ArgumentsDefinition.None;

            return definitions;
        }

        ArgumentsDefinition GetDirectiveArgumentDefinitions(IRuleVisitorContext context)
        {
            var definitions = context
                .Tracker
                .DirectiveDefinition
                ?.Arguments;

            if (definitions == null)
                return ArgumentsDefinition.None;

            return definitions;
        }

        void ValidateArguments(
            ArgumentsDefinition argumentDefinitions,
            IReadOnlyList<Argument> arguments,
            IRuleVisitorContext ruleVisitorContext)
        {
            foreach (var argumentDefinition in argumentDefinitions)
            {
                var type = argumentDefinition.Type;
                var defaultValue = argumentDefinition.DefaultValue;

                if (type is not NonNullType nonNullType || defaultValue != null)
                    continue;

                var argumentName = argumentDefinition.Name.Value;
                var argument = arguments?
                    .SingleOrDefault(a => a.Name == argumentName);

                if (argument == null)
                {
                    ruleVisitorContext.Error(
                        ValidationErrorCodes.R5421RequiredArguments,
                        "Arguments is required. An argument is required " +
                        "if the argument type is non‐null and does not have a default " +
                        "value. Otherwise, the argument is optional. " +
                        $"Argument '{argumentName}' not given");

                    return;
                }

                // variables should be valid
                if (argument.Value is Variable)
                    continue;

                if (argument?.Value == null || argument.Value.Kind == NodeKind.NullValue)
                    ruleVisitorContext.Error(
                        ValidationErrorCodes.R5421RequiredArguments,
                        "Arguments is required. An argument is required " +
                        "if the argument type is non‐null and does not have a default " +
                        "value. Otherwise, the argument is optional. " +
                        $"Value of argument '{argumentName}' cannot be null");
            }
        }

        return (context, rule) =>
        {
            rule.EnterFieldSelection += field =>
            {
                var args = field.Arguments;
                var argumentDefinitions =
                    GetFieldArgumentDefinitions(context);

                ValidateArguments(argumentDefinitions, args ?? Arguments.None, context);
            };
            rule.EnterDirective += directive =>
            {
                var args = directive.Arguments;
                var argumentDefinitions =
                    GetDirectiveArgumentDefinitions(context);

                ValidateArguments(argumentDefinitions, args ?? Arguments.None, context);
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
            rule.EnterFieldSelection += _ => knownArgs = new();
            rule.EnterDirective += _ => knownArgs = new();
            rule.EnterArgument += argument =>
            {
                if (knownArgs.Contains(argument.Name))
                    context.Error(
                        ValidationErrorCodes.R542ArgumentUniqueness,
                        "Fields and directives treat arguments as a mapping of " +
                        "argument name to value. More than one argument with the same " +
                        "name in an argument set is ambiguous and invalid. " +
                        $"Argument: '{argument.Name.Value}'",
                        argument);

                knownArgs.Add(argument.Name);
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
                if (knownFragments.Contains(fragment.FragmentName))
                    context.Error(
                        ValidationErrorCodes.R5511FragmentNameUniqueness,
                        "Fragment definitions are referenced in fragment spreads by name. To avoid " +
                        "ambiguity, each fragment’s name must be unique within a document. " +
                        $"Fragment: '{fragment.FragmentName}'",
                        fragment);

                knownFragments.Add(fragment.FragmentName);
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
                var type = context.Tracker.CurrentType;

                if (type == null)
                    context.Error(
                        ValidationErrorCodes.R5512FragmentSpreadTypeExistence,
                        "Fragments must be specified on types that exist in the schema. This " +
                        "applies for both named and inline fragments. " +
                        $"Fragment: '{node.FragmentName}'",
                        node);
            };
            rule.EnterInlineFragment += node =>
            {
                var type = context.Tracker.CurrentType;

                if (type == null)
                    context.Error(
                        ValidationErrorCodes.R5512FragmentSpreadTypeExistence,
                        "Fragments must be specified on types that exist in the schema. This " +
                        "applies for both named and inline fragments.",
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
                var type = context.Tracker.CurrentType;

                if (type is UnionDefinition)
                    return;

                if (type is InterfaceDefinition or ObjectDefinition)
                    return;

                context.Error(
                    ValidationErrorCodes.R5513FragmentsOnCompositeTypes,
                    "Fragments can only be declared on unions, interfaces, and objects. " +
                    $"Fragment: '{node.FragmentName}'",
                    node);
            };
            rule.EnterInlineFragment += node =>
            {
                var type = context.Tracker.CurrentType;

                if (type is UnionDefinition)
                    return;

                if (type is InterfaceDefinition or ObjectDefinition)
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
            var fragments = new Dictionary<string, FragmentDefinition>();
            var fragmentSpreads = new List<string>();

            rule.EnterFragmentDefinition += fragment => { fragments.Add(fragment.FragmentName, fragment); };
            rule.EnterFragmentSpread += spread => { fragmentSpreads.Add(spread.FragmentName); };
            rule.LeaveDocument += document =>
            {
                foreach (var fragment in fragments)
                {
                    var name = fragment.Key;
                    if (!fragmentSpreads.Contains(name))
                        context.Error(
                            ValidationErrorCodes.R5514FragmentsMustBeUsed,
                            "Defined fragments must be used within a document. " +
                            $"Fragment: '{name}'",
                            fragment.Value);
                }
            };
        };
    }

    public static CombineRule R5521FragmentSpreadTargetDefined()
    {
        return (context, rule) =>
        {
            rule.EnterFragmentSpread += node =>
            {
                var fragment = context.GetFragment(node.FragmentName);

                if (fragment == null)
                    context.Error(
                        ValidationErrorCodes.R5521FragmentSpreadTargetDefined,
                        "Named fragment spreads must refer to fragments " +
                        "defined within the document. " +
                        $"Fragment '{node.FragmentName}' not found");
            };
        };
    }

    public static CombineRule R5522FragmentSpreadsMustNotFormCycles()
    {
        return (context, rule) =>
        {
            var visitedFrags = new List<string>();
            var spreadPath = new Stack<FragmentSpread>();

            // Position in the spread path
            var spreadPathIndexByName = new Dictionary<string, int?>();
            var fragments = context.Document.FragmentDefinitions;

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

        IEnumerable<FragmentSpread> GetFragmentSpreads(SelectionSet node)
        {
            var spreads = new List<FragmentSpread>();

            var setsToVisit = new Stack<SelectionSet>(new[] { node });

            while (setsToVisit.Any())
            {
                var set = setsToVisit.Pop();

                foreach (var selection in set.Selections)
                    if (selection is FragmentSpread spread)
                        spreads.Add(spread);
                    else if (selection is FieldSelection fieldSelection)
                        if (fieldSelection.SelectionSet != null)
                            setsToVisit.Push(fieldSelection.SelectionSet);
            }

            return spreads;
        }

        void DetectCycleRecursive(
            FragmentDefinition fragment,
            Stack<FragmentSpread> spreadPath,
            List<string> visitedFrags,
            Dictionary<string, int?> spreadPathIndexByName,
            IRuleVisitorContext context,
            IReadOnlyCollection<FragmentDefinition>? fragments)
        {
            if (fragments == null)
                return;

            var fragmentName = fragment.FragmentName;
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
                var spreadName = spreadNode.FragmentName;
                var cycleIndex = spreadPathIndexByName.ContainsKey(spreadName)
                    ? spreadPathIndexByName[spreadName]
                    : default;

                spreadPath.Push(spreadNode);

                if (cycleIndex == null)
                {
                    var spreadFragment = fragments.SingleOrDefault(f => f.FragmentName == spreadName);

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
                        .Select(s => s.FragmentName.ToString())
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
            var fragments = context.Document.FragmentDefinitions
                                ?.ToDictionary(f => f.FragmentName)
                            ?? new Dictionary<Name, FragmentDefinition>(0);

            rule.EnterFragmentSpread += node =>
            {
                var fragment = fragments[node.FragmentName];
                var fragmentType = Ast.UnwrapAndResolveType(context.Schema, fragment.TypeCondition);
                var parentType = context.Tracker.ParentType;
                if (fragmentType is not null && parentType is not null)
                {
                    var applicableTypes = false;
                    var parentTypePossibleTypes = GetPossibleTypes(parentType, context.Schema);
                    var fragmentTypePossibleTypes = GetPossibleTypes(fragmentType, context.Schema);

                    if (fragmentType is InterfaceDefinition && parentType is InterfaceDefinition)
                        applicableTypes = parentTypePossibleTypes.Contains(fragmentType);
                    else
                        applicableTypes =
                            fragmentTypePossibleTypes.Intersect(parentTypePossibleTypes).Any();

                    if (!applicableTypes)
                        context.Error(
                            ValidationErrorCodes.R5523FragmentSpreadIsPossible,
                            "Fragments are declared on a type and will only apply " +
                            "when the runtime object type matches the type condition. They " +
                            "also are spread within the context of a parent type. A fragment " +
                            "spread is only valid if its type condition could ever apply within " +
                            "the parent type. " +
                            $"FragmentSpread: '{node.FragmentName}'",
                            node);
                }
                else
                {
                    context.Error(
                        ValidationErrorCodes.R5523FragmentSpreadIsPossible,
                        "Fragments are declared on a type and will only apply " +
                        "when the runtime object type matches the type condition. They " +
                        "also are spread within the context of a parent type. A fragment " +
                        "spread is only valid if its type condition could ever apply within " +
                        "the parent type. " +
                        $"FragmentSpread: '{node.FragmentName}'",
                        node);
                }
            };

            rule.EnterInlineFragment += node =>
            {
                var fragmentType = context.Tracker.CurrentType;
                var parentType = context.Tracker.ParentType;

                if (fragmentType is not null && parentType is not null)
                {
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
                }
                else
                {
                    context.Error(
                        ValidationErrorCodes.R5523FragmentSpreadIsPossible,
                        "Fragments are declared on a type and will only apply " +
                        "when the runtime object type matches the type condition. They " +
                        "also are spread within the context of a parent type. A fragment " +
                        "spread is only valid if its type condition could ever apply within " +
                        "the parent type.",
                        node);
                }
            };
        };

        IEnumerable<TypeDefinition> GetPossibleTypes(TypeDefinition type, ISchema schema)
        {
            switch (type)
            {
                case ObjectDefinition objectDefinition:
                    return new[] { objectDefinition };
                case InterfaceDefinition interfaceType:
                    return schema.GetPossibleTypes(interfaceType);
                case UnionDefinition unionDefinition:
                    return schema.GetPossibleTypes(unionDefinition);
                default:
                    return Enumerable.Empty<TypeDefinition>();
            }
        }
    }

    public static CombineRule R561ValuesOfCorrectType()
    {
        return (context, rule) =>
        {
            //rule.EnterNullValue += node => { };

            rule.EnterListValue += node => { IsValidScalar(context, node); };
            rule.EnterObjectValue += node =>
            {
                var type = context.Tracker.InputType;

                if (type is not InputObjectDefinition inputType)
                {
                    IsValidScalar(context, node);
                    return;
                }

                var fieldNodeMap = node.Fields.ToDictionary(
                    f => f.Name);

                foreach (var fieldDef in context.Schema.GetInputFields(
                             inputType.Name))
                {
                    var fieldNode = fieldNodeMap.ContainsKey(fieldDef.Key);
                    if (!fieldNode && fieldDef.Value.Type is NonNullType NonNullType)
                        context.Error(
                            ValidationErrorCodes.R561ValuesOfCorrectType,
                            RequiredFieldMessage(
                                type.ToString(),
                                fieldDef.Key,
                                NonNullType.ToString()),
                            (INode)node);
                }
            };
            rule.EnterObjectField += node =>
            {
                var parentType = context.Tracker.ParentInputType;

                var fieldType = context.Tracker.InputType;
                if (fieldType == null && parentType is InputObjectDefinition)
                    context.Error(
                        ValidationErrorCodes.R561ValuesOfCorrectType,
                        UnknownFieldMessage(
                            parentType.Name,
                            node.Name,
                            string.Empty),
                        node);
            };
            rule.EnterEnumValue += node =>
            {
                var maybeEnumType = context.Tracker.InputType;

                if (maybeEnumType is not EnumDefinition type)
                    IsValidScalar(context, node);

                else
                    try
                    {
                        var value = new EnumConverter(type).ParseLiteral(node);

                        if (value is null)
                            throw new ValueCoercionException(
                                $"{Printer.Print(type)} does not contain a value of '{Printer.Print(node)}'", node,
                                node);
                    }
                    catch (Exception x)
                    {
                        context.Error(
                            ValidationErrorCodes.R561ValuesOfCorrectType,
                            BadValueMessage(
                                type.Name,
                                Printer.Print(node),
                                string.Empty));
                    }
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
            return $"Expected type '{typeName}', found '{valueName}' " +
                   message;
        }

        string RequiredFieldMessage(
            string typeName,
            string fieldName,
            string fieldTypeName
        )
        {
            return $"Field '{typeName}.{fieldName}' of required type " +
                   $"'{fieldTypeName}' was not provided.";
        }

        string UnknownFieldMessage(
            string typeName,
            string fieldName,
            string message
        )
        {
            return $"Field '{fieldName}' is not defined by type '{typeName}' " +
                   message;
        }

        void IsValidScalar(
            IRuleVisitorContext context,
            ValueBase node)
        {
            var locationType = context.Tracker.InputType;

            if (locationType == null)
                return;


            if (locationType is not ScalarDefinition type)
            {
                context.Error(
                    ValidationErrorCodes.R561ValuesOfCorrectType,
                    BadValueMessage(
                        locationType.Name,
                        Printer.Print(node),
                        string.Empty),
                    node);

                return;
            }

            try
            {
                var converter = context.Schema.GetValueConverter(type.Name) ?? throw new ValueCoercionException(
                    $"Value converter for '{Printer.Print(type)}' not found from schema.",
                    type,
                    type);

                converter.ParseLiteral(node);
            }
            catch (Exception e)
            {
                context.Error(
                    ValidationErrorCodes.R561ValuesOfCorrectType,
                    BadValueMessage(locationType.Name,
                        Printer.Print(node),
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
                var inputFieldName = inputField.Name;

                if (context.Tracker.ParentInputType is not InputObjectDefinition parentType)
                    return;

                var inputFieldDefinition = context.Schema
                    .GetInputField(parentType.Name, inputFieldName);

                if (inputFieldDefinition == null)
                    context.Error(
                        ValidationErrorCodes.R562InputObjectFieldNames,
                        "Every input field provided in an input object " +
                        "value must be defined in the set of possible fields of " +
                        "that input object’s expected type. " +
                        $"Field: '{inputField.Name}'",
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
                    var name = inputField.Name;
                    if (fields.Count(f => f.Name == name) > 1)
                        context.Error(
                            ValidationErrorCodes.R563InputObjectFieldUniqueness,
                            "Input objects must not contain more than one field " +
                            "of the same name, otherwise an ambiguity would exist which " +
                            "includes an ignored portion of syntax. " +
                            $"Field: '{name}'",
                            fields.Where(f => f.Name == name));
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
                var inputObject = context.Tracker.InputType as InputObjectDefinition;

                if (inputObject == null)
                    return;

                var fields = node.Fields.ToDictionary(f => f.Name);
                var fieldDefinitions = context.Schema.GetInputFields(inputObject.Name);

                foreach (var fieldDefinition in fieldDefinitions)
                {
                    var type = fieldDefinition.Value.Type;
                    var defaultValue = fieldDefinition.Value.DefaultValue;

                    if (type is NonNullType nonNullType && defaultValue == null)
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
                                $"Field '{nonNullType}.{fieldName}' is required.",
                                (INode)node);

                            return;
                        }

                        if (field.Value.Kind == NodeKind.NullValue)
                            context.Error(
                                ValidationErrorCodes.R564InputObjectRequiredFields,
                                "Input object fields may be required. Much like a field " +
                                "may have required arguments, an input object may have required " +
                                "fields. An input field is required if it has a non‐null type and " +
                                "does not have a default value. Otherwise, the input object field " +
                                "is optional. " +
                                $"Field '{nonNullType}.{field}' value cannot be null.",
                                node, field);
                    }
                }
            };
        };
    }


    /// <summary>
    ///     5.7.1, 5.7.3
    /// </summary>
    /// <returns></returns>
    public static CombineRule R571And573Directives()
    {
        return (context, rule) =>
        {
            rule.EnterDirective += directive =>
            {
                var directiveName = directive.Name;
                var directiveDefinition = context.Schema.GetDirectiveType(directiveName);

                if (directiveDefinition == null)
                    context.Error(
                        ValidationErrorCodes.R571DirectivesAreDefined,
                        "GraphQL servers define what directives they support. " +
                        "For each usage of a directive, the directive must be available " +
                        "on that server. " +
                        $"Directive: '{directiveName}'",
                        directive);
            };

            rule.EnterOperationDefinition += node => CheckDirectives(context, node.Directives);
            rule.EnterFieldSelection += node => CheckDirectives(context, node.Directives);
            rule.EnterFragmentDefinition += node => CheckDirectives(context, node.Directives);
            rule.EnterFragmentSpread += node => CheckDirectives(context, node.Directives);
            rule.EnterInlineFragment += node => CheckDirectives(context, node.Directives);
        };

        // 5.7.3
        void CheckDirectives(IRuleVisitorContext context, Language.Nodes.Directives? directives)
        {
            if (directives is null || directives.Count == 0)
                return;

            var knownDirectives = new List<string>();

            foreach (var directive in directives)
            {
                if (knownDirectives.Contains(directive.Name))
                    context.Error(
                        ValidationErrorCodes.R573DirectivesAreUniquePerLocation,
                        "For each usage of a directive, the directive must be used in a " +
                        "location that the server has declared support for. " +
                        $"Directive '{directive.Name.Value}' is used multiple times on same location",
                        directive);

                knownDirectives.Add(directive.Name);
            }
        }
    }

    public static CombineRule R572DirectivesAreInValidLocations()
    {
        return (context, rule) =>
        {
            rule.EnterOperationDefinition += node => CheckDirectives(context, node, node.Directives);
            rule.EnterFieldSelection += node => CheckDirectives(context, node, node.Directives);
            rule.EnterFragmentDefinition += node => CheckDirectives(context, node, node.Directives);
            rule.EnterFragmentSpread += node => CheckDirectives(context, node, node.Directives);
            rule.EnterInlineFragment += node => CheckDirectives(context, node, node.Directives);
        };

        // 5.7.2
        void CheckDirectives(
            IRuleVisitorContext context,
            INode node,
            Language.Nodes.Directives? directives)
        {
            if (directives == null || directives.Count == 0)
                return;

            var currentLocation = GetLocation(node);
            foreach (var directive in directives)
            {
                var directiveType = context.Schema.GetDirectiveType(directive.Name);

                if (directiveType is null)
                    continue;

                var validLocations = directiveType.DirectiveLocations;

                if (!validLocations.Contains(currentLocation))
                    context.Error(
                        ValidationErrorCodes.R572DirectivesAreInValidLocations,
                        "GraphQL servers define what directives they support " +
                        "and where they support them. For each usage of a directive, " +
                        "the directive must be used in a location that the server has " +
                        "declared support for. " +
                        $"Directive '{directive.Name.Value}' is in invalid location " +
                        $"'{currentLocation}'. Valid locations: '{string.Join(",", validLocations)}'",
                        directive);
            }
        }

        string GetLocation(INode appliedTo)
        {
            switch (appliedTo.Kind)
            {
                case NodeKind.OperationDefinition:
                    switch (((OperationDefinition)appliedTo).Operation)
                    {
                        case OperationType.Query:
                            return ExecutableDirectiveLocations.QUERY;
                        case OperationType.Mutation:
                            return ExecutableDirectiveLocations.MUTATION;
                        case OperationType.Subscription:
                            return ExecutableDirectiveLocations.SUBSCRIPTION;
                    }

                    break;
                case NodeKind.FieldSelection:
                    return ExecutableDirectiveLocations.FIELD;
                case NodeKind.FragmentSpread:
                    return ExecutableDirectiveLocations.FRAGMENT_SPREAD;
                case NodeKind.InlineFragment:
                    return ExecutableDirectiveLocations.INLINE_FRAGMENT;
                case NodeKind.FragmentDefinition:
                    return ExecutableDirectiveLocations.FRAGMENT_DEFINITION;
                case NodeKind.VariableDefinition:
                    throw new InvalidOperationException("Not supported");
                case NodeKind.SchemaDefinition:
                case NodeKind.SchemaExtension:
                    return TypeSystemDirectiveLocations.SCHEMA;
                case NodeKind.ScalarDefinition:
                    //case NodeKind.TypeExtension:
                    return TypeSystemDirectiveLocations.SCALAR;
                case NodeKind.ObjectDefinition:
                    //case NodeKind.OBJECT_TYPE_EXTENSION:
                    return TypeSystemDirectiveLocations.OBJECT;
                case NodeKind.FieldDefinition:
                    return TypeSystemDirectiveLocations.FIELD_DEFINITION;
                case NodeKind.InterfaceDefinition:
                    //case NodeKind.INTERFACE_TYPE_EXTENSION:
                    return TypeSystemDirectiveLocations.INTERFACE;
                case NodeKind.UnionDefinition:
                    //case NodeKind.UNION_TYPE_EXTENSION:
                    return TypeSystemDirectiveLocations.UNION;
                case NodeKind.EnumDefinition:
                    //case NodeKind.ENUM_TYPE_EXTENSION:
                    return TypeSystemDirectiveLocations.ENUM;
                case NodeKind.EnumValueDefinition:
                    return TypeSystemDirectiveLocations.ENUM_VALUE;
                case NodeKind.InputObjectDefinition:
                    //case NodeKind.INPUT_OBJECT_TYPE_EXTENSION:
                    return TypeSystemDirectiveLocations.INPUT_OBJECT;
                case NodeKind.Argument:
                    return TypeSystemDirectiveLocations.ARGUMENT_DEFINITION;
                case NodeKind.InputValueDefinition:
                    return TypeSystemDirectiveLocations.INPUT_FIELD_DEFINITION;
            }

            throw new InvalidOperationException($"Not supported location: {appliedTo.Kind}");
        }
    }


    /// <summary>
    ///     5.8.1, 5.8.2
    /// </summary>
    /// <returns></returns>
    public static CombineRule R581And582Variables()
    {
        return (context, rule) =>
        {
            rule.EnterOperationDefinition += node =>
            {
                if (node.VariableDefinitions == null)
                    return;

                var knownVariables = new List<string>();
                foreach (var variableUsage in node.VariableDefinitions)
                {
                    var variable = variableUsage.Variable;
                    var variableName = variable.Name;

                    // 5.8.1 Variable Uniqueness
                    if (knownVariables.Contains(variableName))
                        context.Error(
                            ValidationErrorCodes.R581VariableUniqueness,
                            "If any operation defines more than one " +
                            "variable with the same name, it is ambiguous and " +
                            "invalid. It is invalid even if the type of the " +
                            "duplicate variable is the same. " +
                            $"Variable: '{variableName}'",
                            node);

                    knownVariables.Add(variableName);

                    // 5.8.2
                    var variableType = Ast.UnwrapAndResolveType(context.Schema, variableUsage.Type);
                    if (variableType is null || !TypeIs.IsInputType(variableType))
                        context.Error(
                            ValidationErrorCodes.R582VariablesAreInputTypes,
                            "Variables can only be input types. Objects, unions, " +
                            "and interfaces cannot be used as inputs.. " +
                            $"Given type of '{variableName}' is '{variableType}'",
                            node);
                }
            };
        };
    }

    public static CombineRule R583AllVariableUsesDefined()
    {
        return (context, rule) =>
        {
            var variableDefinitions = new List<string>();
            rule.EnterOperationDefinition += node =>
            {
                variableDefinitions.Clear();
                if (node.VariableDefinitions != null)
                    variableDefinitions.AddRange(node.VariableDefinitions.Select(v => v.Variable.Name.Value));
            };

            rule.LeaveOperationDefinition += node =>
            {
                var usages = context.GetRecursiveVariables(node)
                    .Select(usage => usage.Node.Name.Value)
                    .ToList();

                foreach (var usage in usages)
                    if (!variableDefinitions.Contains(usage))
                        context.Error(
                            ValidationErrorCodes.R583AllVariableUsesDefined,
                            "Variables are scoped on a per‐operation basis. " +
                            "That means that any variable used within the context of " +
                            "an operation must be defined at the top level of that operation. " +
                            $"Variable use '{usage}' is not defined.",
                            node
                        );
            };
        };
    }

    public static CombineRule R584AllVariablesUsed()
    {
        return (context, rule) =>
        {
            var variableDefinitions = new List<VariableDefinition>();

            rule.EnterVariableDefinition += node => variableDefinitions.Add(node);
            rule.EnterOperationDefinition += node => { variableDefinitions.Clear(); };
            rule.LeaveOperationDefinition += node =>
            {
                var usages = context.GetRecursiveVariables(node)
                    .Select(usage => usage.Node.Name.Value)
                    .ToList();

                foreach (var variableDefinition in variableDefinitions)
                {
                    var variableName = variableDefinition.Variable.Name.Value;

                    if (!usages.Contains(variableName))
                        context.Error(
                            ValidationErrorCodes.R584AllVariablesUsed,
                            "All variables defined by an operation " +
                            "must be used in that operation or a fragment " +
                            "transitively included by that operation. Unused " +
                            "variables cause a validation error. " +
                            $"Variable: '{variableName}' is not used.",
                            variableDefinition
                        );
                }
            };
        };
    }

    public static CombineRule R585AllVariableUsagesAreAllowed()
    {
        return (context, rule) =>
        {
            var variableDefinitions = new Dictionary<string, VariableDefinition>();
            var usages = new List<(Variable Variable, TypeBase? Type, ValueBase? DefaultValue)>();

            rule.EnterVariableDefinition += node =>
                variableDefinitions[node.Variable.Name] = node;
            rule.EnterOperationDefinition += node => { variableDefinitions.Clear(); };

            rule.EnterVariable += node =>
            {
                var type = context.Tracker.ArgumentDefinition?.Type;
                var defaultValue = context.Tracker.DefaultValue;
                usages.Add((node, type, defaultValue));
            };

            rule.LeaveOperationDefinition += node =>
            {
                //var usages = context.GetRecursiveVariables(node);

                foreach (var usage in usages)
                {
                    var variableName = usage.Variable.Name;

                    if (!variableDefinitions.TryGetValue(variableName, out var variableDefinition)) return;

                    if (!AllowedVariableUsage(
                            context.Schema,
                            variableDefinition.Type,
                            variableDefinition?.DefaultValue,
                            usage.Type,
                            usage.DefaultValue))
                        context.Error(
                            ValidationErrorCodes.R585AllVariableUsagesAreAllowed,
                            "Variable usages must be compatible with the arguments they are passed to. " +
                            $"Variable '{variableName}' of type '{variableDefinition?.Type}' used in " +
                            $"position expecting type '{usage.Type}'");
                }
            };
        };

        bool AllowedVariableUsage(
            ISchema schema,
            TypeBase varType,
            object? varDefaultValue,
            TypeBase? locationType,
            object? locationDefaultValue
        )
        {
            if (locationType is NonNullType nonNullTypeTypeLocationType && varType is not NonNullType)
            {
                var hasNonNullTypeTypeVariableDefaultValue = varDefaultValue != null;
                var hasLocationDefaultValue = locationDefaultValue != null;

                if (!hasNonNullTypeTypeVariableDefaultValue && !hasLocationDefaultValue) return false;

                var nullableLocationType = nonNullTypeTypeLocationType.OfType;
                return IsTypeSubTypeOf(schema, varType, nullableLocationType);
            }

            return IsTypeSubTypeOf(schema, varType, locationType);
        }

        //todo: Move to TypeIs
        bool IsTypeSubTypeOf(
            ISchema schema,
            TypeBase maybeSubType,
            TypeBase superType
        )
        {
            // Equivalent type is a valid subtype
            if (maybeSubType is NamedType namedSubType && superType is NamedType namedSuperType)
                if (namedSubType.Name.Equals(namedSuperType.Name))
                    return true;

            // If superType is non-null, maybeSubType must also be non-null.
            if (superType is NonNullType nonNullTypeTypeSuperType)
            {
                if (maybeSubType is NonNullType nonNullTypeTypeMaybeSubType)
                    return IsTypeSubTypeOf(
                        schema,
                        nonNullTypeTypeMaybeSubType.OfType,
                        nonNullTypeTypeSuperType.OfType);

                return false;
            }

            if (maybeSubType is NonNullType nonNullTypeTypeMaybeSubType2)
                // If superType is nullable, maybeSubType may be non-null or nullable.
                return IsTypeSubTypeOf(schema, nonNullTypeTypeMaybeSubType2.OfType, superType);

            // If superType type is a list, maybeSubType type must also be a list.
            if (superType is ListType listSuperType)
            {
                if (maybeSubType is ListType listMaybeSubType)
                    return IsTypeSubTypeOf(
                        schema,
                        listMaybeSubType.OfType,
                        listSuperType.OfType);

                return false;
            }

            if (maybeSubType is ListType)
                // If superType is not a list, maybeSubType must also be not a list.
                return false;

            // If superType type is an abstract type, maybeSubType type may be a currently
            // possible object type.
            var superTypeDefinition = schema.GetNamedType(superType.Unwrap().Name);
            var maybeSubTypeDefinition = schema.GetNamedType(maybeSubType.Unwrap().Name);

            var possibleTypes = superTypeDefinition switch
            {
                null => Enumerable.Empty<TypeDefinition>(),
                InterfaceDefinition interfaceDefinition => schema.GetPossibleTypes(interfaceDefinition),
                UnionDefinition unionDefinition => schema.GetPossibleTypes(unionDefinition),
                _ => Enumerable.Empty<TypeDefinition>()
            };

            return possibleTypes.Contains(maybeSubTypeDefinition);
        }
    }
}
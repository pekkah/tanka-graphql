using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation;

public class Visitor
{
    private readonly List<FragmentDefinition> _fragments = new();

    public IReadOnlyCollection<FragmentDefinition> Fragments => _fragments;


    public virtual Argument BeginVisitArgument(Argument argument)
    {
        if (argument.Value != null)
            BeginVisitNode(argument.Value);

        return EndVisitArgument(argument);
    }

    public virtual IEnumerable<Argument> BeginVisitArguments(
        IEnumerable<Argument> arguments)
    {
        foreach (var node in arguments)
            BeginVisitNode(node);

        return arguments;
    }

    public virtual BooleanValue BeginVisitBooleanValue(
        BooleanValue value)
    {
        return value;
    }

    public virtual Directive BeginVisitDirective(Directive directive)
    {
        if (directive.Arguments != null)
            BeginVisitArguments(directive.Arguments);

        return directive;
    }

    public virtual IEnumerable<Directive> BeginVisitDirectives(
        IEnumerable<Directive> directives)
    {
        foreach (var directive in directives)
            BeginVisitNode(directive);
        return directives;
    }

    public virtual EnumValue BeginVisitEnumValue(EnumValue value)
    {
        return value;
    }

    public virtual FieldSelection BeginVisitFieldSelection(
        FieldSelection selection)
    {
        if (selection.Arguments != null)
            BeginVisitArguments(selection.Arguments);
        if (selection.SelectionSet != null)
            BeginVisitNode(selection.SelectionSet);
        if (selection.Directives != null)
            BeginVisitDirectives(selection.Directives);
        return EndVisitFieldSelection(selection);
    }

    public virtual FloatValue BeginVisitFloatValue(
        FloatValue value)
    {
        return value;
    }

    public virtual FragmentDefinition BeginVisitFragmentDefinition(
        FragmentDefinition node)
    {
        BeginVisitNode(node.TypeCondition);
        if (node.SelectionSet != null)
            BeginVisitNode(node.SelectionSet);

        _fragments.Add(node);
        return node;
    }

    public virtual FragmentSpread BeginVisitFragmentSpread(
        FragmentSpread fragmentSpread)
    {
        return fragmentSpread;
    }

    public virtual InlineFragment BeginVisitInlineFragment(
        InlineFragment inlineFragment)
    {
        if (inlineFragment.TypeCondition != null)
            BeginVisitNode(inlineFragment.TypeCondition);
        if (inlineFragment.Directives != null)
            BeginVisitDirectives(inlineFragment.Directives);
        if (inlineFragment.SelectionSet != null)
            BeginVisitSelectionSet(inlineFragment.SelectionSet);
        return inlineFragment;
    }

    public virtual IntValue BeginVisitIntValue(IntValue value)
    {
        return value;
    }

    public virtual NamedType BeginVisitNamedType(
        NamedType typeCondition)
    {
        return typeCondition;
    }

    public virtual INode BeginVisitNode(INode node)
    {
        switch (node.Kind)
        {
            case NodeKind.OperationDefinition:
                return BeginVisitOperationDefinition((OperationDefinition)node);
            case NodeKind.VariableDefinition:
                return BeginVisitVariableDefinition((VariableDefinition)node);
            case NodeKind.Variable:
                return BeginVisitVariable((Variable)node);
            case NodeKind.SelectionSet:
                return BeginVisitSelectionSet((SelectionSet)node);
            case NodeKind.FieldSelection:
                return BeginVisitNonIntrospectionFieldSelection((FieldSelection)node);
            case NodeKind.Argument:
                return BeginVisitArgument((Argument)node);
            case NodeKind.FragmentSpread:
                return BeginVisitFragmentSpread((FragmentSpread)node);
            case NodeKind.InlineFragment:
                return BeginVisitInlineFragment((InlineFragment)node);
            case NodeKind.FragmentDefinition:
                return BeginVisitFragmentDefinition((FragmentDefinition)node);
            case NodeKind.IntValue:
                return BeginVisitIntValue((IntValue)node);
            case NodeKind.FloatValue:
                return BeginVisitFloatValue((FloatValue)node);
            case NodeKind.StringValue:
                return BeginVisitStringValue((StringValue)node);
            case NodeKind.BooleanValue:
                return BeginVisitBooleanValue((BooleanValue)node);
            case NodeKind.EnumValue:
                return BeginVisitEnumValue((EnumValue)node);
            case NodeKind.ListValue:
                return BeginVisitListValue((ListValue)node);
            case NodeKind.ObjectValue:
                return BeginVisitObjectValue((ObjectValue)node);
            case NodeKind.ObjectField:
                return BeginVisitObjectField((ObjectField)node);
            case NodeKind.Directive:
                return BeginVisitDirective((Directive)node);
            case NodeKind.NamedType:
                return BeginVisitNamedType((NamedType)node);
            default:
                return node;
        }
    }

    public virtual ObjectField BeginVisitObjectField(ObjectField node)
    {
        BeginVisitNode(node.Value);
        return node;
    }

    public virtual OperationDefinition BeginVisitOperationDefinition(
        OperationDefinition definition)
    {
        if (definition.VariableDefinitions != null)
            BeginVisitVariableDefinitions(definition.VariableDefinitions);
        BeginVisitNode(definition.SelectionSet);
        return EndVisitOperationDefinition(definition);
    }

    public virtual OperationDefinition EndVisitOperationDefinition(
        OperationDefinition definition)
    {
        return definition;
    }

    public virtual SelectionSet BeginVisitSelectionSet(
        SelectionSet selectionSet)
    {
        foreach (var selection in selectionSet)
            BeginVisitNode(selection);

        return selectionSet;
    }

    public virtual StringValue BeginVisitStringValue(
        StringValue value)
    {
        return value;
    }

    public virtual Variable BeginVisitVariable(Variable variable)
    {
        return EndVisitVariable(variable);
    }

    public virtual VariableDefinition BeginVisitVariableDefinition(
        VariableDefinition node)
    {
        BeginVisitNode(node.Type);
        return node;
    }

    public virtual IEnumerable<VariableDefinition> BeginVisitVariableDefinitions(
        IEnumerable<VariableDefinition> variableDefinitions)
    {
        foreach (var variableDefinition in variableDefinitions)
            BeginVisitNode(variableDefinition);

        return variableDefinitions;
    }

    public virtual Argument EndVisitArgument(Argument argument)
    {
        return argument;
    }

    public virtual FieldSelection EndVisitFieldSelection(
        FieldSelection selection)
    {
        return selection;
    }

    public virtual Variable EndVisitVariable(Variable variable)
    {
        return variable;
    }

    public virtual void Visit(ExecutableDocument ast)
    {
        if (ast.FragmentDefinitions != null)
            foreach (var definition in ast.FragmentDefinitions)
                BeginVisitNode(definition);

        if (ast.OperationDefinitions != null)
            foreach (var definition in ast.OperationDefinitions)
                BeginVisitNode(definition);
    }

    public virtual ObjectValue BeginVisitObjectValue(
        ObjectValue node)
    {
        foreach (var objectField in node) BeginVisitNode(objectField);
        return EndVisitObjectValue(node);
    }

    public virtual ObjectValue EndVisitObjectValue(ObjectValue node)
    {
        return node;
    }

    public virtual ListValue EndVisitListValue(ListValue node)
    {
        return node;
    }

    public virtual ListValue BeginVisitListValue(ListValue node)
    {
        foreach (var node1 in node)
            BeginVisitNode(node1);

        return EndVisitListValue(node);
    }

    private FieldSelection BeginVisitNonIntrospectionFieldSelection(FieldSelection selection)
    {
        return BeginVisitFieldSelection(selection);
    }
}
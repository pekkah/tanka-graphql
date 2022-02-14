using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public class ReadOnlyDocumentWalker<TContext>
    where TContext : DocumentWalkerContextBase
{
    private readonly TContext _context;
    private readonly IReadOnlyList<IReadOnlyDocumentVisitor<TContext>> _visitors;

    public ReadOnlyDocumentWalker(IReadOnlyList<IReadOnlyDocumentVisitor<TContext>> visitors, TContext context)
    {
        _visitors = visitors;
        _context = context;
    }

    public virtual void Visit(INode? node)
    {
        switch (node)
        {
            case null:
                break;
            case Argument argument:
                VisitArgument(argument);
                break;
            case Arguments arguments:
                VisitCollection(arguments);
                break;
            case BooleanValue booleanValue:
                VisitBooleanValue(booleanValue);
                break;
            case DefaultValue defaultValue:
                VisitDefaultValue(defaultValue);
                break;
            case Directive directive:
                VisitDirective(directive);
                break;
            case Directives directives:
                VisitCollection(directives);
                break;
            case EnumValue enumValue:
                VisitEnumValue(enumValue);
                break;
            case ExecutableDocument executableDocument:
                VisitExecutableDocument(executableDocument);
                break;
            case FieldSelection fieldSelection:
                VisitFieldSelection(fieldSelection);
                break;
            case FloatValue floatValue:
                VisitFloatValue(floatValue);
                break;
            case FragmentDefinition fragmentDefinition:
                VisitFragmentDefinition(fragmentDefinition);
                break;
            case FragmentDefinitions fragmentDefinitions:
                VisitCollection(fragmentDefinitions);
                break;
            case FragmentSpread fragmentSpread:
                VisitFragmentSpread(fragmentSpread);
                break;
            case InlineFragment inlineFragment:
                VisitInlineFragment(inlineFragment);
                break;
            case IntValue intValue:
                VisitIntValue(intValue);
                break;
            case ListType listType:
                VisitListType(listType);
                break;
            case ListValue listValue:
                VisitListValue(listValue);
                break;
            case NamedType namedType:
                VisitNamedType(namedType);
                break;
            case NonNullType nonNullType:
                VisitNonNullType(nonNullType);
                break;
            case NullValue nullValue:
                VisitNullValue(nullValue);
                break;
            case ObjectField objectField:
                VisitObjectField(objectField);
                break;
            case ObjectValue objectValue:
                VisitObjectValue(objectValue);
                break;
            case OperationDefinition operationDefinition:
                VisitOperationDefinition(operationDefinition);
                break;
            case OperationDefinitions operationDefinitions:
                VisitCollection(operationDefinitions);
                break;
            case SelectionSet selectionSet:
                VisitSelectionSet(selectionSet);
                break;
            case StringValue stringValue:
                VisitStringValue(stringValue);
                break;
            case ArgumentsDefinition argumentsDefinition:
                VisitCollection(argumentsDefinition);
                break;
            case DirectiveDefinition directiveDefinition:
                VisitDirectiveDefinition(directiveDefinition);
                break;
            case EnumDefinition enumDefinition:
                VisitEnumDefinition(enumDefinition);
                break;
            case EnumValueDefinition enumValueDefinition:
                VisitEnumValueDefinition(enumValueDefinition);
                break;
            case EnumValuesDefinition enumValuesDefinition:
                VisitCollection(enumValuesDefinition);
                break;
            case FieldDefinition fieldDefinition:
                VisitFieldDefinition(fieldDefinition);
                break;
            case FieldsDefinition fieldsDefinition:
                VisitCollection(fieldsDefinition);
                break;
            case ImplementsInterfaces implementsInterfaces:
                VisitCollection(implementsInterfaces);
                break;
            case Import import:
                VisitImport(import);
                break;
            case InputObjectDefinition inputObjectDefinition:
                VisitInputObjectDefinition(inputObjectDefinition);
                break;
            case InputValueDefinition inputValueDefinition:
                VisitInputValueDefinition(inputValueDefinition);
                break;
            case InterfaceDefinition interfaceDefinition:
                VisitInterfaceDefinition(interfaceDefinition);
                break;
            case ObjectDefinition objectDefinition:
                VisitObjectDefinition(objectDefinition);
                break;
            case RootOperationTypeDefinition rootOperationTypeDefinition:
                VisitRootOperationTypeDefinition(rootOperationTypeDefinition);
                break;
            case ScalarDefinition scalarDefinition:
                VisitScalarDefinition(scalarDefinition);
                break;
            case SchemaDefinition schemaDefinition:
                VisitSchemaDefinition(schemaDefinition);
                break;
            case SchemaExtension schemaExtension:
                VisitSchemaExtension(schemaExtension);
                break;
            case UnionDefinition unionDefinition:
                VisitUnionDefinition(unionDefinition);
                break;
            case UnionMemberTypes unionMemberTypes:
                VisitCollection(unionMemberTypes);
                break;
            case TypeExtension typeExtension:
                VisitTypeExtension(typeExtension);
                break;
            case TypeSystemDocument typeSystemDocument:
                VisitTypeSystemDocument(typeSystemDocument);
                break;
            case Variable variable:
                VisitVariable(variable);
                break;
            case VariableDefinition variableDefinition:
                VisitVariableDefinition(variableDefinition);
                break;
            case VariableDefinitions variableDefinitions:
                VisitCollection(variableDefinitions);
                break;
            case InputFieldsDefinition inputFieldsDefinition:
                VisitCollection(inputFieldsDefinition);
                break;
            case RootOperationTypeDefinitions rootOperationTypeDefinitions:
                VisitCollection(rootOperationTypeDefinitions);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(node), node.GetType().Name, "Node not supported");
        }
    }

    protected virtual void ExitNode(INode node)
    {
        foreach (var visitor in _visitors) visitor.ExitNode(_context, node);

        _context.Pop();
    }

    protected virtual void EnterNode(INode node)
    {
        _context.Push(node);
        foreach (var visitor in _visitors) visitor.EnterNode(_context, node);
    }

    private void VisitRootOperationTypeDefinition(RootOperationTypeDefinition node)
    {
        EnterNode(node);
        Visit(node.NamedType);
        ExitNode(node);
    }

    private void VisitImport(Import node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitArgument(Argument node)
    {
        EnterNode(node);

        Visit(node.Value);

        ExitNode(node);
    }

    private void VisitBooleanValue(BooleanValue node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitDefaultValue(DefaultValue node)
    {
        EnterNode(node);
        Visit(node.Value);
        ExitNode(node);
    }

    private void VisitEnumValue(EnumValue node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitDirective(Directive node)
    {
        EnterNode(node);

        Visit(node.Arguments);

        ExitNode(node);
    }

    private void VisitExecutableDocument(ExecutableDocument node)
    {
        EnterNode(node);

        Visit(node.FragmentDefinitions);
        Visit(node.OperationDefinitions);

        ExitNode(node);
    }

    private void VisitFieldSelection(FieldSelection node)
    {
        EnterNode(node);

        Visit(node.Arguments);
        Visit(node.Directives);

        if (node.SelectionSet != null)
            Visit(node.SelectionSet);

        ExitNode(node);
    }

    private void VisitFloatValue(FloatValue node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitFragmentDefinition(FragmentDefinition node)
    {
        EnterNode(node);

        Visit(node.TypeCondition);
        Visit(node.Directives);
        Visit(node.SelectionSet);

        ExitNode(node);
    }

    private void VisitFragmentSpread(FragmentSpread node)
    {
        EnterNode(node);

        Visit(node.Directives);

        ExitNode(node);
    }

    private void VisitInlineFragment(InlineFragment node)
    {
        EnterNode(node);

        if (node.TypeCondition != null)
            Visit(node.TypeCondition);

        Visit(node.Directives);

        Visit(node.SelectionSet);

        ExitNode(node);
    }

    private void VisitIntValue(IntValue node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitListType(ListType node)
    {
        EnterNode(node);
        Visit(node.OfType);
        ExitNode(node);
    }

    private void VisitListValue(ListValue node)
    {
        VisitCollection(node);
    }

    private void VisitNamedType(NamedType node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitNonNullType(NonNullType node)
    {
        EnterNode(node);
        Visit(node.OfType);
        ExitNode(node);
    }

    private void VisitNullValue(NullValue node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitObjectField(ObjectField node)
    {
        EnterNode(node);
        Visit(node.Value);
        ExitNode(node);
    }

    private void VisitObjectValue(ObjectValue node)
    {
        VisitCollection(node);
    }

    private void VisitOperationDefinition(OperationDefinition node)
    {
        EnterNode(node);

        Visit(node.VariableDefinitions);

        Visit(node.Directives);

        Visit(node.SelectionSet);

        ExitNode(node);
    }

    private void VisitSelectionSet(SelectionSet node)
    {
        VisitCollection(node);
    }

    private void VisitStringValue(StringValue node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitDirectiveDefinition(DirectiveDefinition node)
    {
        EnterNode(node);

        Visit(node.Arguments);

        ExitNode(node);
    }

    private void VisitEnumDefinition(EnumDefinition node)
    {
        EnterNode(node);

        Visit(node.Directives);

        Visit(node.Values);

        ExitNode(node);
    }

    private void VisitEnumValueDefinition(EnumValueDefinition node)
    {
        EnterNode(node);

        Visit(node.Value);

        Visit(node.Directives);

        ExitNode(node);
    }

    private void VisitObjectDefinition(ObjectDefinition node)
    {
        EnterNode(node);
        Visit(node.Interfaces);
        Visit(node.Directives);
        Visit(node.Fields);
        ExitNode(node);
    }

    private void VisitFieldDefinition(FieldDefinition node)
    {
        EnterNode(node);

        Visit(node.Arguments);

        Visit(node.Type);

        Visit(node.Directives);

        ExitNode(node);
    }

    private void VisitInputValueDefinition(InputValueDefinition node)
    {
        EnterNode(node);

        Visit(node.Type);

        if (node.DefaultValue != null)
            Visit(node.DefaultValue);

        Visit(node.Directives);

        ExitNode(node);
    }

    private void VisitInterfaceDefinition(InterfaceDefinition node)
    {
        EnterNode(node);

        Visit(node.Interfaces);
        Visit(node.Directives);
        Visit(node.Fields);

        ExitNode(node);
    }

    private void VisitInputObjectDefinition(InputObjectDefinition node)
    {
        EnterNode(node);

        Visit(node.Directives);

        Visit(node.Fields);

        ExitNode(node);
    }

    private void VisitScalarDefinition(ScalarDefinition node)
    {
        EnterNode(node);

        Visit(node.Directives);

        ExitNode(node);
    }

    private void VisitSchemaDefinition(SchemaDefinition node)
    {
        EnterNode(node);

        Visit(node.Directives);
        Visit(node.Operations);

        ExitNode(node);
    }

    private void VisitSchemaExtension(SchemaExtension node)
    {
        EnterNode(node);

        Visit(node.Directives);
        Visit(node.Operations);

        ExitNode(node);
    }

    private void VisitUnionDefinition(UnionDefinition node)
    {
        EnterNode(node);

        Visit(node.Directives);
        Visit(node.Members);

        ExitNode(node);
    }

    private void VisitTypeExtension(TypeExtension node)
    {
        EnterNode(node);

        Visit(node.Definition);

        ExitNode(node);
    }

    private void VisitTypeSystemDocument(TypeSystemDocument node)
    {
        EnterNode(node);

        if (node.Imports != null)
            foreach (var import in node.Imports)
                Visit(import);

        if (node.DirectiveDefinitions != null)
            foreach (var definition in node.DirectiveDefinitions)
                Visit(definition);

        if (node.TypeDefinitions != null)
            foreach (var definition in node.TypeDefinitions)
                Visit(definition);

        if (node.SchemaDefinitions != null)
            foreach (var definition in node.SchemaDefinitions)
                Visit(definition);

        if (node.TypeExtensions != null)
            foreach (var extension in node.TypeExtensions)
                Visit(extension);

        if (node.SchemaExtensions != null)
            foreach (var extension in node.SchemaExtensions)
                Visit(extension);

        ExitNode(node);
    }

    private void VisitVariable(Variable node)
    {
        EnterNode(node);
        ExitNode(node);
    }

    private void VisitVariableDefinition(VariableDefinition node)
    {
        EnterNode(node);

        Visit(node.Variable);
        Visit(node.Type);
        Visit(node.DefaultValue);
        Visit(node.Directives);

        ExitNode(node);
    }

    private void VisitCollection(ICollectionNode<INode>? nodes)
    {
        if (nodes == null)
            return;

        var arrayState = _context.PushArrayState(nodes);
        EnterNode(nodes);

        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            arrayState.Index = i;
            Visit(node);
        }

        ExitNode(nodes);
        _context.PopArrayState();
    }
}
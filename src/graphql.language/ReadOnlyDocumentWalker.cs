using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public abstract class DocumentWalkerContextBase
    {
        private Stack<INode> _nodes { get; } = new Stack<INode>();

        public IEnumerable<INode> Nodes => _nodes;
        public INode Top => _nodes.Peek();

        public INode? Parent { get; private set; }

        public void Push(INode node)
        {
            if (_nodes.Count > 0)
                Parent = _nodes.Peek();
            
            _nodes.Push(node);
        }

        public INode Pop() => _nodes.Pop();
        public bool Contains(INode node) => _nodes.Contains(node);
    }

    public class ReadOnlyDocumentWalker<TContext>
        where TContext : DocumentWalkerContextBase
    {
        private readonly IReadOnlyList<IReadOnlyDocumentVisitor<TContext>> _visitors;
        private readonly TContext _context;

        public ReadOnlyDocumentWalker(IReadOnlyList<IReadOnlyDocumentVisitor<TContext>> visitors, TContext context)
        {
            _visitors = visitors;
            _context = context;
        }

        public virtual void Visit(INode node)
        {
            switch (node)
            {
                case null:
                    break;
                case Argument argument:
                    VisitArgument(argument);
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
                case SelectionSet selectionSet:
                    VisitSelectionSet(selectionSet);
                    break;
                case StringValue stringValue:
                    VisitStringValue(stringValue);
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
                case FieldDefinition fieldDefinition:
                    VisitFieldDefinition(fieldDefinition);
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
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

            VisitCollection(node.Arguments);

            ExitNode(node);
        }

        private void VisitExecutableDocument(ExecutableDocument node)
        {
            EnterNode(node);

            VisitCollection(node.FragmentDefinitions);
            VisitCollection(node.OperationDefinitions);

            ExitNode(node);
        }

        private void VisitFieldSelection(FieldSelection node)
        {
            EnterNode(node);

            VisitCollection(node.Arguments);
            VisitCollection(node.Directives);

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

            VisitCollection(node.Directives);

            Visit(node.TypeCondition);
            Visit(node.SelectionSet);

            ExitNode(node);
        }

        private void VisitFragmentSpread(FragmentSpread node)
        {
            EnterNode(node);

            VisitCollection(node.Directives);

            ExitNode(node);
        }

        private void VisitInlineFragment(InlineFragment node)
        {
            EnterNode(node);

            if (node.TypeCondition != null)
                Visit(node.TypeCondition);

            VisitCollection(node.Directives);

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
            EnterNode(node);

            VisitCollection(node.Values);

            ExitNode(node);
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
            EnterNode(node);

            VisitCollection(node.Fields);

            ExitNode(node);
        }

        private void VisitOperationDefinition(OperationDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.VariableDefinitions);

            VisitCollection(node.Directives);

            Visit(node.SelectionSet);

            ExitNode(node);
        }

        private void VisitSelectionSet(SelectionSet node)
        {
            EnterNode(node);

            VisitCollection(node.Selections);

            ExitNode(node);
        }

        private void VisitStringValue(StringValue node)
        {
            EnterNode(node);
            ExitNode(node);
        }

        private void VisitDirectiveDefinition(DirectiveDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.Arguments);

            ExitNode(node);
        }

        private void VisitEnumDefinition(EnumDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.Directives);

            VisitCollection(node.Values);

            ExitNode(node);
        }

        private void VisitEnumValueDefinition(EnumValueDefinition node)
        {
            EnterNode(node);

            Visit(node.Value);

            VisitCollection(node.Directives);

            ExitNode(node);
        }

        private void VisitObjectDefinition(ObjectDefinition node)
        {
            EnterNode(node);
            ExitNode(node);
        }

        private void VisitFieldDefinition(FieldDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.Arguments);

            Visit(node.Type);

            VisitCollection(node.Directives);

            ExitNode(node);
        }

        private void VisitInputValueDefinition(InputValueDefinition node)
        {
            EnterNode(node);

            Visit(node.Type);

            if (node.DefaultValue != null)
                Visit(node.DefaultValue);

            VisitCollection(node.Directives);

            ExitNode(node);
        }

        private void VisitInterfaceDefinition(InterfaceDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.Interfaces);
            VisitCollection(node.Directives);
            VisitCollection(node.Fields);

            ExitNode(node);
        }

        private void VisitInputObjectDefinition(InputObjectDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.Directives);

            VisitCollection(node.Fields);

            ExitNode(node);
        }

        private void VisitScalarDefinition(ScalarDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.Directives);

            ExitNode(node);
        }

        private void VisitSchemaDefinition(SchemaDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.Directives);

            foreach (var operation in node.Operations)
            {
                Visit(operation.NamedType);
            }

            ExitNode(node);
        }

        private void VisitSchemaExtension(SchemaExtension node)
        {
            EnterNode(node);

            VisitCollection(node.Directives);

            if (node.Operations != null)
                foreach (var operation in node.Operations)
                {
                    Visit(operation.NamedType);
                }

            ExitNode(node);
        }

        private void VisitUnionDefinition(UnionDefinition node)
        {
            EnterNode(node);

            VisitCollection(node.Directives);
            VisitCollection(node.Members);

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

            VisitCollection(node.Imports);
            
            VisitCollection(node.DirectiveDefinitions);
            VisitCollection(node.TypeDefinitions);
            VisitCollection(node.SchemaDefinitions);
            
            VisitCollection(node.TypeExtensions);
            VisitCollection(node.SchemaExtensions);

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

            VisitCollection(node.Directives);

            ExitNode(node);
        }

        public void VisitCollection(IReadOnlyCollection<INode>? nodes)
        {
            if (nodes == null)
                return;

            var wrapper = (IReadOnlyCollectionNode<INode>)new ReadOnlyCollectionNode<INode>(nodes);
            EnterNode(wrapper);

            foreach (var node in nodes)
            {
                Visit(node);
            }
            
            ExitNode(wrapper);
        }

        protected virtual void ExitNode(INode node)
        {
            foreach (var visitor in _visitors)
            {
                visitor.ExitNode(_context, node);
            }

            _context.Pop();
        }

        protected virtual void EnterNode(INode node)
        {
            _context.Push(node);
            foreach (var visitor in _visitors)
            {
                visitor.EnterNode(_context, node);
            }
        }
    }
}
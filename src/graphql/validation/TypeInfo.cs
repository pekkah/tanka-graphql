using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;
using GraphQLParser.AST;
using static tanka.graphql.type.Ast;

namespace tanka.graphql.validation
{
    public class TypeInfo : INodeVisitor
    {
        private readonly Stack<ASTNode> _ancestorStack = new Stack<ASTNode>();
        private readonly Stack<IField> _fieldDefStack = new Stack<IField>();
        private readonly Stack<IType> _inputTypeStack = new Stack<IType>();
        private readonly Stack<INamedType> _parentTypeStack = new Stack<INamedType>();
        private readonly ISchema _schema;

        private readonly Stack<IType> _typeStack = new Stack<IType>();

        //private DirectiveType _directive;
        private Argument _argument;
        private DirectiveType _directive;

        public TypeInfo(ISchema schema)
        {
            _schema = schema;
        }

        public DirectiveType GetDirective()
        {
            return _directive;
        }

        public void Enter(ASTNode node)
        {
            _ancestorStack.Push(node);

            if (node is GraphQLSelectionSet)
            {
                _parentTypeStack.Push(GetLastType());
                return;
            }

            if (node is GraphQLFieldSelection fieldSelection)
            {
                var parentType = GetParentType().Unwrap();
                var field = GetFieldDef(_schema, parentType, fieldSelection);

                _fieldDefStack.Push(field);
                var targetType = field?.Type;
                _typeStack.Push(targetType);
                return;
            }

            if (node is GraphQLDirective directive)
            {
                _directive = _schema.GetDirective(directive.Name.Value);
            }

            if (node is GraphQLOperationDefinition op)
            {
                INamedType type = null;
                if (op.Operation == OperationType.Query)
                    type = _schema.Query;
                else if (op.Operation == OperationType.Mutation)
                    type = _schema.Mutation;
                else if (op.Operation == OperationType.Subscription) type = _schema.Subscription;
                _typeStack.Push(type);
                return;
            }

            if (node is GraphQLFragmentDefinition fragmentDefinition)
            {
                var type = _schema.GetNamedType(fragmentDefinition.TypeCondition.Name.Value);
                _typeStack.Push(type);
                return;
            }

            if (node is GraphQLInlineFragment inlineFragment)
            {
                var type = inlineFragment.TypeCondition != null
                    ? _schema.GetNamedType(inlineFragment.TypeCondition.Name.Value)
                    : GetLastType();

                _typeStack.Push(type);
                return;
            }

            if (node is GraphQLVariableDefinition varDef)
            {
                var inputType = TypeFromAst(_schema, varDef.Type);
                _inputTypeStack.Push(inputType);
                return;
            }

            if (node is GraphQLArgument argAst)
            {
                Argument argDef = null;
                IType argType = null;

                var args = GetDirective() != null ? GetDirective()?.Arguments : GetFieldDef()?.Arguments;

                if (args != null)
                {
                    argDef = args.SingleOrDefault(a => a.Key == argAst.Name.Value).Value;
                    argType = argDef?.Type;
                }

                _argument = argDef;
                _inputTypeStack.Push(argType);
            }

            if (node is GraphQLListValue)
            {
                var type = GetInputType().Unwrap();
                _inputTypeStack.Push(type);
            }

            if (node is GraphQLObjectField objectField)
            {
                var objectType = GetInputType().Unwrap();
                IType fieldType = null;

                if (objectType is InputObjectType inputObjectType)
                {
                    var inputField = _schema.GetInputField(inputObjectType.Name, objectField.Name.Value);
                    fieldType = inputField?.Type;
                }

                _inputTypeStack.Push(fieldType);
            }
        }

        public void Leave(ASTNode node)
        {
            _ancestorStack.Pop();

            if (node is GraphQLSelectionSet)
            {
                _parentTypeStack.Pop();
                return;
            }

            if (node is GraphQLFieldSelection)
            {
                _fieldDefStack.Pop();
                _typeStack.Pop();
                return;
            }

            if (node is GraphQLDirective)
            {
                _directive = null;
                return;
            };

            if (node is GraphQLOperationDefinition
                || node is GraphQLFragmentDefinition
                || node is GraphQLInlineFragment)
            {
                _typeStack.Pop();
                return;
            }

            if (node is GraphQLVariableDefinition)
            {
                _inputTypeStack.Pop();
                return;
            }

            if (node is GraphQLArgument)
            {
                _argument = null;
                _inputTypeStack.Pop();
                return;
            }

            if (node is GraphQLListValue || node is GraphQLObjectField)
            {
                _inputTypeStack.Pop();
            }
        }

        public ASTNode[] GetAncestors()
        {
            return _ancestorStack.Select(x => x).Skip(1).Reverse().ToArray();
        }

        public INamedType GetLastType()
        {
            var type = _typeStack.Any() ? _typeStack.Peek() : null;

            return ResolveNamedReference(type);
        }

        private INamedType ResolveNamedReference(IType type)
        {
            if (type == null)
                return null;

            if (type is NamedTypeReference typeRef)
            {
                return ResolveNamedReference(typeRef.TypeName);
            }

            return type as INamedType;
        }

        private INamedType ResolveNamedReference(string typeName)
        {
            var type = _schema.GetNamedType(typeName);
            return type;
        }

        public IType GetInputType()
        {
            return _inputTypeStack.Any() ? _inputTypeStack.Peek() : null;
        }

        public INamedType GetParentType()
        {
            var type = _parentTypeStack.Any() ? _parentTypeStack.Peek() : null;

            return ResolveNamedReference(type.Unwrap());
        }

        public IField GetFieldDef()
        {
            return _fieldDefStack.Any() ? _fieldDefStack.Peek() : null;
        }

        /*public DirectiveType GetDirective()
        {
            return _directive;
        }*/

        public Argument GetArgument()
        {
            return _argument;
        }

        private IField GetFieldDef(ISchema schema, IType parentType, GraphQLFieldSelection fieldSelection)
        {
            var name = fieldSelection.Name.Value;

            if (parentType is ComplexType complexType)
            {
                return schema.GetField(complexType.Name, name);
            }

            return null;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using fugu.graphql.type;
using GraphQLParser.AST;
using static fugu.graphql.type.Ast;

namespace fugu.graphql.validation
{
    public class TypeInfo : INodeVisitor
    {
        private readonly Stack<ASTNode> _ancestorStack = new Stack<ASTNode>();
        private readonly Stack<IField> _fieldDefStack = new Stack<IField>();
        private readonly Stack<IGraphQLType> _inputTypeStack = new Stack<IGraphQLType>();
        private readonly Stack<IGraphQLType> _parentTypeStack = new Stack<IGraphQLType>();
        private readonly ISchema _schema;

        private readonly Stack<IGraphQLType> _typeStack = new Stack<IGraphQLType>();

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
                IGraphQLType type = null;
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
                IGraphQLType argType = null;

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
                IGraphQLType fieldType = null;

                if (objectType is InputObjectType inputObjectType)
                {
                    var inputField = inputObjectType.GetField(objectField.Name.Value);
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

        public IGraphQLType GetLastType()
        {
            var type = _typeStack.Any() ? _typeStack.Peek() : null;

            return ResolveNamedReference(type);
        }

        private IGraphQLType ResolveNamedReference(IGraphQLType type)
        {
            if (type == null)
                return type;

            if (type is NamedTypeReference typeRef)
            {
                return ResolveNamedReference(typeRef.TypeName);
            }

            return type;
        }

        private IGraphQLType ResolveNamedReference(string typeName)
        {
            var type = _schema.GetNamedType(typeName);
            return type;
        }

        public IGraphQLType GetInputType()
        {
            return _inputTypeStack.Any() ? _inputTypeStack.Peek() : null;
        }

        public IGraphQLType GetParentType()
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

        private IField GetFieldDef(ISchema schema, IGraphQLType parentType, GraphQLFieldSelection fieldSelection)
        {
            var name = fieldSelection.Name.Value;

            if (parentType is ComplexType complexType)
            {
                return complexType.GetField(name);
            }

            return null;
        }
    }
}
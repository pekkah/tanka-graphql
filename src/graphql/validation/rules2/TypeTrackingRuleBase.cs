using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    public abstract class TypeTrackingRuleBase : RuleBase
    {
        private readonly Stack<object> _defaultValueStack = new Stack<object>();

        private readonly Stack<(string Name, IField Field)?> _fieldDefStack = new Stack<(string Name, IField Field)?>();

        private readonly Stack<IType> _inputTypeStack = new Stack<IType>();

        private readonly Stack<ComplexType> _parentTypeStack = new Stack<ComplexType>();

        private readonly Stack<IType> _typeStack = new Stack<IType>();
        private Argument _argument;

        private DirectiveType _directive;

        private object _enumValue;

        public override IEnumerable<ASTNodeKind> AppliesToNodeKinds => new[]
        {
            ASTNodeKind.SelectionSet,
            ASTNodeKind.Field,
            ASTNodeKind.OperationDefinition,
            ASTNodeKind.InlineFragment,
            ASTNodeKind.FragmentDefinition,
            ASTNodeKind.Directive,
            ASTNodeKind.VariableDefinition,
            ASTNodeKind.Argument,
            ASTNodeKind.ListValue,
            ASTNodeKind.ObjectField,
            ASTNodeKind.EnumValue
        };

        public override void BeginVisitSelectionSet(GraphQLSelectionSet selectionSet,
            IValidationContext context)
        {
            var namedType = GetNamedType(GetCurrentType());
            var complexType = namedType as ComplexType;
            _parentTypeStack.Push(complexType);
        }

        public override void BeginVisitFieldSelection(GraphQLFieldSelection selection,
            IValidationContext context)
        {
            var parentType = GetParentType();
            (string Name, IField Field)? fieldDef = null;
            IType fieldType = null;

            if (parentType != null)
            {
                fieldDef = getFieldDef(context.Schema, parentType, selection);

                if (fieldDef != null) fieldType = fieldDef.Value.Field.Type;
            }

            _fieldDefStack.Push(fieldDef);
            _typeStack.Push(TypeIs.IsOutputType(fieldType) ? fieldType : null);
        }

        public override void BeginVisitDirective(GraphQLDirective directive,
            IValidationContext context)
        {
            _directive = context.Schema.GetDirective(directive.Name.Value);
        }

        public override void BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition, IValidationContext context)
        {
            ObjectType type = null;
            switch (definition.Operation)
            {
                case OperationType.Query:
                    type = context.Schema.Query;
                    break;
                case OperationType.Mutation:
                    type = context.Schema.Mutation;
                    break;
                case OperationType.Subscription:
                    type = context.Schema.Subscription;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _typeStack.Push(type);
        }

        public override void BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment,
            IValidationContext context)
        {
            var typeConditionAst = inlineFragment.TypeCondition;

            IType outputType;
            if (typeConditionAst != null)
            {
                outputType = Ast.TypeFromAst(context.Schema, typeConditionAst);
            }
            else
            {
                outputType = GetNamedType(GetCurrentType());
            }

            _typeStack.Push(TypeIs.IsOutputType(outputType) ? outputType : null);
        }

        public override void BeginVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context)
        {
            var typeConditionAst = node.TypeCondition;

            IType outputType;
            if (typeConditionAst != null)
            {
                outputType = Ast.TypeFromAst(context.Schema, typeConditionAst);
            }
            else
            {
                outputType = GetNamedType(GetCurrentType());
            }

            _typeStack.Push(TypeIs.IsOutputType(outputType) ? outputType : null);
        }

        public override void BeginVisitVariableDefinition(GraphQLVariableDefinition node,
            IValidationContext context)
        {
            var inputType = Ast.TypeFromAst(context.Schema, node.Type);
            _inputTypeStack.Push(TypeIs.IsInputType(inputType) ? inputType : null);
        }

        public override void BeginVisitArgument(GraphQLArgument argument,
            IValidationContext context)
        {
            Argument argDef = null;
            IType argType = null;

            if (GetDirective() != null)
            {
                argDef = GetDirective()?.GetArgument(argument.Name.Value);
                argType = argDef?.Type;
            }
            else if (GetFieldDef() != null)
            {
                argDef = GetFieldDef()?.Field.GetArgument(argument.Name.Value);
                argType = argDef?.Type;
            }

            _argument = argDef;
            _defaultValueStack.Push(argDef?.DefaultValue);
            _inputTypeStack.Push(TypeIs.IsInputType(argType) ? argType : null);
        }

        public override void BeginVisitListValue(GraphQLListValue node,
            IValidationContext context)
        {
            var listType = GetNullableType(GetInputType());
            var itemType = listType is List list ? list.WrappedType : listType;

            // List positions never have a default value
            _defaultValueStack.Push(null);
            _inputTypeStack.Push(TypeIs.IsInputType(itemType) ? itemType : null);
        }

        public override void BeginVisitObjectField(GraphQLObjectField node,
            IValidationContext context)
        {
            var objectType = GetNamedType(GetInputType());
            IType inputFieldType = null;
            InputObjectField inputField = null;

            if (objectType is InputObjectType inputObjectType)
            {
                inputField = context.Schema.GetInputField(
                    inputObjectType.Name,
                    node.Name.Value);

                if (inputField != null)
                    inputFieldType = inputField.Type;
            }

            _defaultValueStack.Push(inputField?.DefaultValue);
            _inputTypeStack.Push(TypeIs.IsInputType(inputFieldType) ? inputFieldType : null);
        }

        public override void BeginVisitEnumValue(GraphQLScalarValue value,
            IValidationContext context)
        {
            var maybeEnumType = GetNamedType(GetInputType());
            object enumValue = null;
            if (maybeEnumType is EnumType enumType) enumValue = enumType.ParseLiteral(value);

            _enumValue = enumValue;
        }

        public override void EndVisitSelectionSet(GraphQLSelectionSet selectionSet,
            IValidationContext context)
        {
            _parentTypeStack.Pop();
        }

        public override void EndVisitFieldSelection(GraphQLFieldSelection selection,
            IValidationContext context)
        {
            _fieldDefStack.Pop();
            _typeStack.Pop();
        }

        public override void EndVisitDirective(GraphQLDirective directive,
            IValidationContext context)
        {
            _directive = null;
        }

        public override void EndVisitOperationDefinition(GraphQLOperationDefinition definition,
            IValidationContext context)
        {
            _typeStack.Pop();
        }

        public override void EndVisitInlineFragment(GraphQLInlineFragment inlineFragment,
            IValidationContext context)
        {
            _typeStack.Pop();
        }

        public override void EndVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context)
        {
            _typeStack.Pop();
        }

        public override void EndVisitVariableDefinition(GraphQLVariableDefinition node,
            IValidationContext context)
        {
            _inputTypeStack.Pop();
        }

        public override void EndVisitArgument(GraphQLArgument argument,
            IValidationContext context)
        {
            _argument = null;
            _defaultValueStack.Pop();
            _inputTypeStack.Pop();
        }

        public override void EndVisitListValue(GraphQLListValue node,
            IValidationContext context)
        {
            _defaultValueStack.Pop();
            _inputTypeStack.Pop();
        }

        public override void EndVisitObjectField(GraphQLObjectField node,
            IValidationContext context)
        {
            _defaultValueStack.Pop();
            _inputTypeStack.Pop();
        }

        public override void EndVisitEnumValue(GraphQLScalarValue value,
            IValidationContext context)
        {
            _enumValue = null;
        }

        protected IType GetCurrentType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _typeStack.Peek();
        }

        protected ComplexType GetParentType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _parentTypeStack.Peek();
        }

        //todo: originally returns an input type
        protected IType GetInputType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _inputTypeStack.Peek();
        }

        protected IType GetParentInputType()
        {
            //todo: probably a bad idea
            return _inputTypeStack.ElementAtOrDefault(_inputTypeStack.Count - 2);
        }

        protected (string Name, IField Field)? GetFieldDef()
        {
            if (_fieldDefStack.Count == 0)
                return null;

            return _fieldDefStack.Peek();
        }

        protected object GetDefaultValue()
        {
            if (_defaultValueStack.Count == 0)
                return null;

            return _defaultValueStack.Peek();
        }

        protected DirectiveType GetDirective()
        {
            return _directive;
        }

        protected Argument GetArgument()
        {
            return _argument;
        }

        protected object GetEnumValue()
        {
            return _enumValue;
        }

        protected IType GetNamedType(IType type)
        {
            return type?.Unwrap();
        }

        protected IType GetNullableType(IType type)
        {
            if (type is NonNull nonNull)
                return nonNull.WrappedType;

            return null;
        }

        private (string Name, IField Field)? getFieldDef(
            ISchema schema,
            IType parentType,
            GraphQLFieldSelection fieldNode)
        {
            var name = fieldNode.Name.Value;
            /*if (name == SchemaMetaFieldDef.name 
                         && schema.getQueryType() == parentType) 
            {
                return SchemaMetaFieldDef;
            }

            if (name == TypeMetaFieldDef.name 
                         && schema.getQueryType() == parentType) 
            {
                return TypeMetaFieldDef;
            }

            if (name == TypeNameMetaFieldDef.name 
                         && isCompositeType(parentType)) 
            {
                return TypeNameMetaFieldDef;
            }*/

            if (parentType is ComplexType complexType)
            {
                var field = schema.GetField(complexType.Name, name);

                if (field == null)
                    return null;

                return (name, field);
            }

            return null;
        }
    }
}
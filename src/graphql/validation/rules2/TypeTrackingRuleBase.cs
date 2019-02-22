using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    public abstract class TypeTrackingRuleBase : Rule
    {
        private Argument _argument;

        private readonly Stack<object> _defaultValueStack = new Stack<object>();

        private DirectiveType _directive;

        private object _enumValue;

        private readonly Stack<(string Name, IField Field)?> _fieldDefStack = new Stack<(string Name, IField Field)?>();

        private readonly Stack<IType> _inputTypeStack = new Stack<IType>();

        private readonly Stack<ComplexType> _parentTypeStack = new Stack<ComplexType>();

        private readonly Stack<IType> _typeStack = new Stack<IType>();

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

        public override IEnumerable<ValidationError> BeginVisitSelectionSet(GraphQLSelectionSet selectionSet,
            IValidationContext context)
        {
            var namedType = getNamedType(getType());
            var complexType = namedType as ComplexType;
            _parentTypeStack.Push(complexType);

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitFieldSelection(GraphQLFieldSelection selection,
            IValidationContext context)
        {
            var parentType = getParentType();
            (string Name, IField Field)? fieldDef = null;
            IType fieldType = null;

            if (parentType != null)
            {
                fieldDef = getFieldDef(context.Schema, parentType, selection);

                if (fieldDef != null) fieldType = fieldDef.Value.Field.Type;
            }

            _fieldDefStack.Push(fieldDef);
            _typeStack.Push(TypeIs.IsOutputType(fieldType) ? fieldType : null);

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitDirective(GraphQLDirective directive,
            IValidationContext context)
        {
            _directive = context.Schema.GetDirective(directive.Name.Value);

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitOperationDefinition(
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

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment,
            IValidationContext context)
        {
            var typeConditionAst = inlineFragment.TypeCondition;
            var outputType = Ast.TypeFromAst(context.Schema, typeConditionAst)
                             ?? getNamedType(getType());
            _typeStack.Push(TypeIs.IsOutputType(outputType) ? outputType : null);

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context)
        {
            var typeConditionAst = node.TypeCondition;
            var outputType = Ast.TypeFromAst(context.Schema, typeConditionAst)
                             ?? getNamedType(getType());
            _typeStack.Push(TypeIs.IsOutputType(outputType) ? outputType : null);

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitVariableDefinition(GraphQLVariableDefinition node,
            IValidationContext context)
        {
            var inputType = Ast.TypeFromAst(context.Schema, node.Type);
            _inputTypeStack.Push(TypeIs.IsInputType(inputType) ? inputType : null);

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitArgument(GraphQLArgument argument,
            IValidationContext context)
        {
            Argument argDef = null;
            IType argType = null;

            if (getDirective() != null)
            {
                argDef = getDirective()?.GetArgument(argument.Name.Value);
                argType = argDef?.Type;
            }
            else if (getFieldDef() != null)
            {
                argDef = getFieldDef()?.Field.GetArgument(argument.Name.Value);
                argType = argDef?.Type;
            }

            _argument = argDef;
            _defaultValueStack.Push(argDef?.DefaultValue);
            _inputTypeStack.Push(TypeIs.IsInputType(argType) ? argType : null);

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitListValue(GraphQLListValue node,
            IValidationContext context)
        {
            var listType = getNullableType(getInputType());
            var itemType = listType is List list ? list.WrappedType : listType;

            // List positions never have a default value
            _defaultValueStack.Push(null);
            _inputTypeStack.Push(TypeIs.IsInputType(itemType) ? itemType : null);

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitObjectField(GraphQLObjectField node,
            IValidationContext context)
        {
            var objectType = getNamedType(getInputType());
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

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitEnumValue(GraphQLScalarValue value,
            IValidationContext context)
        {
            var maybeEnumType = getNamedType(getInputType());
            object enumValue = null;
            if (maybeEnumType is EnumType enumType) enumValue = enumType.ParseLiteral(value);

            _enumValue = enumValue;

            yield break;
        }

        public override IEnumerable<ValidationError> EndVisitSelectionSet(GraphQLSelectionSet selectionSet,
            IValidationContext context)
        {
            _parentTypeStack.Pop();

            return base.EndVisitSelectionSet(selectionSet, context);
        }

        public override IEnumerable<ValidationError> EndVisitFieldSelection(GraphQLFieldSelection selection,
            IValidationContext context)
        {
            _fieldDefStack.Pop();
            _typeStack.Pop();

            return base.EndVisitFieldSelection(selection, context);
        }

        public override IEnumerable<ValidationError> EndVisitDirective(GraphQLDirective directive,
            IValidationContext context)
        {
            _directive = null;
            return base.EndVisitDirective(directive, context);
        }

        public override IEnumerable<ValidationError> EndVisitOperationDefinition(GraphQLOperationDefinition definition,
            IValidationContext context)
        {
            _typeStack.Pop();
            return base.EndVisitOperationDefinition(definition, context);
        }

        public override IEnumerable<ValidationError> EndVisitInlineFragment(GraphQLInlineFragment inlineFragment,
            IValidationContext context)
        {
            _typeStack.Pop();
            return base.EndVisitInlineFragment(inlineFragment, context);
        }

        public override IEnumerable<ValidationError> EndVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context)
        {
            _typeStack.Pop();
            return base.EndVisitFragmentDefinition(node, context);
        }

        public override IEnumerable<ValidationError> EndVisitVariableDefinition(GraphQLVariableDefinition node,
            IValidationContext context)
        {
            _inputTypeStack.Pop();
            return base.EndVisitVariableDefinition(node, context);
        }

        public override IEnumerable<ValidationError> EndVisitArgument(GraphQLArgument argument,
            IValidationContext context)
        {
            _argument = null;
            _defaultValueStack.Pop();
            _inputTypeStack.Pop();
            return base.EndVisitArgument(argument, context);
        }

        public override IEnumerable<ValidationError> EndVisitListValue(GraphQLListValue node,
            IValidationContext context)
        {
            _defaultValueStack.Pop();
            _inputTypeStack.Pop();
            return base.EndVisitListValue(node, context);
        }

        public override IEnumerable<ValidationError> EndVisitObjectField(GraphQLObjectField node,
            IValidationContext context)
        {
            _defaultValueStack.Pop();
            _inputTypeStack.Pop();
            return base.EndVisitObjectField(node, context);
        }

        public override IEnumerable<ValidationError> EndVisitEnumValue(GraphQLScalarValue value,
            IValidationContext context)
        {
            _enumValue = null;
            return base.EndVisitEnumValue(value, context);
        }

        protected IType getType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _typeStack.Peek();
        }

        protected ComplexType getParentType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _parentTypeStack.Peek();
        }

        //todo: originally returns an input type
        protected IType getInputType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _inputTypeStack.Peek();
        }

        protected IType getParentInputType()
        {
            //todo: probably a bad idea
            return _inputTypeStack.ElementAtOrDefault(_inputTypeStack.Count - 2);
        }

        protected (string Name, IField Field)? getFieldDef()
        {
            if (_fieldDefStack.Count == 0)
                return null;

            return _fieldDefStack.Peek();
        }

        protected object getDefaultValue()
        {
            if (_defaultValueStack.Count == 0)
                return null;

            return _defaultValueStack.Peek();
        }

        protected DirectiveType getDirective()
        {
            return _directive;
        }

        protected Argument getArgument()
        {
            return _argument;
        }

        protected object getEnumValue()
        {
            return _enumValue;
        }

        protected IType getNamedType(IType type)
        {
            return type?.Unwrap();
        }

        protected IType getNullableType(IType type)
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
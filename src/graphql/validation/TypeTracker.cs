using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.type;

namespace tanka.graphql.validation
{
    public class TypeTracker : RuleVisitor
    {
        private readonly Stack<object> _defaultValueStack = new Stack<object>();

        private readonly Stack<(string Name, IField Field)?> _fieldDefStack = new Stack<(string Name, IField Field)?>();

        private readonly Stack<IType> _inputTypeStack = new Stack<IType>();

        private readonly Stack<ComplexType> _parentTypeStack = new Stack<ComplexType>();

        private readonly Stack<IType> _typeStack = new Stack<IType>();
        private Argument _argument;

        private DirectiveType _directive;

        private object _enumValue;

        public TypeTracker(ISchema schema)
        {
            EnterSelectionSet = selectionSet =>
            {
                var namedType = GetNamedType(GetCurrentType());
                var complexType = namedType as ComplexType;
                _parentTypeStack.Push(complexType);
            };

            EnterFieldSelection = selection =>
            {
                var parentType = GetParentType();
                (string Name, IField Field)? fieldDef = null;
                IType fieldType = null;

                if (parentType != null)
                {
                    fieldDef = GetFieldDef(schema, parentType, selection);

                    if (fieldDef != null) fieldType = fieldDef.Value.Field.Type;
                }

                _fieldDefStack.Push(fieldDef);
                _typeStack.Push(TypeIs.IsOutputType(fieldType) ? fieldType : null);
            };

            EnterDirective = directive =>
            {
                _directive = schema.GetDirectiveType(directive.Name.Value);
            };

            EnterOperationDefinition = definition =>
            {
                ObjectType type = null;
                switch (definition.Operation)
                {
                    case OperationType.Query:
                        type = schema.Query;
                        break;
                    case OperationType.Mutation:
                        type = schema.Mutation;
                        break;
                    case OperationType.Subscription:
                        type = schema.Subscription;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _typeStack.Push(type);
            };

            EnterInlineFragment = inlineFragment =>
            {
                var typeConditionAst = inlineFragment.TypeCondition;

                IType outputType;
                if (typeConditionAst != null)
                    outputType = Ast.TypeFromAst(schema, typeConditionAst);
                else
                    outputType = GetNamedType(GetCurrentType());

                _typeStack.Push(TypeIs.IsOutputType(outputType) ? outputType : null);
            };

            EnterFragmentDefinition = node =>
            {
                var typeConditionAst = node.TypeCondition;

                IType outputType;
                if (typeConditionAst != null)
                    outputType = Ast.TypeFromAst(schema, typeConditionAst);
                else
                    outputType = GetNamedType(GetCurrentType());

                _typeStack.Push(TypeIs.IsOutputType(outputType) ? outputType : null);
            };

            EnterVariableDefinition = node =>
            {
                var inputType = Ast.TypeFromAst(schema, node.Type);
                _inputTypeStack.Push(TypeIs.IsInputType(inputType) ? inputType : null);
            };

            EnterArgument = argument =>
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
            };

            EnterListValue = node =>
            {
                var listType = GetNullableType(GetInputType());
                var itemType = listType is List list ? list.OfType : listType;

                // List positions never have a default value
                _defaultValueStack.Push(null);
                _inputTypeStack.Push(TypeIs.IsInputType(itemType) ? itemType : null);
            };

            EnterObjectField = node =>
            {
                var objectType = GetNamedType(GetInputType());
                IType inputFieldType = null;
                InputObjectField inputField = null;

                if (objectType is InputObjectType inputObjectType)
                {
                    inputField = schema.GetInputField(
                        inputObjectType.Name,
                        node.Name.Value);

                    if (inputField != null)
                        inputFieldType = inputField.Type;
                }

                _defaultValueStack.Push(inputField?.DefaultValue);
                _inputTypeStack.Push(TypeIs.IsInputType(inputFieldType) ? inputFieldType : null);
            };

            EnterEnumValue = value =>
            {
                var maybeEnumType = GetNamedType(GetInputType());
                object enumValue = null;

                if (maybeEnumType is EnumType enumType)
                    enumValue = enumType.ParseLiteral(value);

                _enumValue = enumValue;
            };

            LeaveSelectionSet = _ => _parentTypeStack.Pop();

            LeaveFieldSelection = _ =>
            {
                _fieldDefStack.Pop();
                _typeStack.Pop();
            };

            LeaveDirective = _ => _directive = null;

            LeaveOperationDefinition = _ => _typeStack.Pop();

            LeaveInlineFragment = _ => _typeStack.Pop();

            LeaveFragmentDefinition = _ => _typeStack.Pop();

            LeaveVariableDefinition = _ => _inputTypeStack.Pop();

            LeaveArgument = _ =>
            {
                _argument = null;
                _defaultValueStack.Pop();
                _inputTypeStack.Pop();
            };

            LeaveListValue = _ =>
            {
                _defaultValueStack.Pop();
                _inputTypeStack.Pop();
            };

            LeaveObjectField = _ =>
            {
                _defaultValueStack.Pop();
                _inputTypeStack.Pop();
            };

            LeaveEnumValue = _ => _enumValue = null;
        }

        public IType GetCurrentType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _typeStack.Peek();
        }

        public ComplexType GetParentType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _parentTypeStack.Peek();
        }

        //todo: originally returns an input type
        public IType GetInputType()
        {
            if (_typeStack.Count == 0)
                return null;

            return _inputTypeStack.Peek();
        }

        public IType GetParentInputType()
        {
            //todo: probably a bad idea
            return _inputTypeStack.ElementAtOrDefault(_inputTypeStack.Count - 1);
        }

        public (string Name, IField Field)? GetFieldDef()
        {
            if (_fieldDefStack.Count == 0)
                return null;

            return _fieldDefStack.Peek();
        }

        public object GetDefaultValue()
        {
            if (_defaultValueStack.Count == 0)
                return null;

            return _defaultValueStack.Peek();
        }

        public DirectiveType GetDirective()
        {
            return _directive;
        }

        public Argument GetArgument()
        {
            return _argument;
        }

        public object GetEnumValue()
        {
            return _enumValue;
        }

        public IType GetNamedType(IType type)
        {
            return type?.Unwrap();
        }

        public IType GetNullableType(IType type)
        {
            if (type is NonNull nonNull)
                return nonNull.OfType;

            return null;
        }

        public (string Name, IField Field)? GetFieldDef(
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
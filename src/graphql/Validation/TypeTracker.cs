using System;
using System.Collections.Generic;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation;

public class TypeTracker : RuleVisitor
{
    public TypeTracker(ISchema schema)
    {
        EnterOperationDefinition = node =>
        {
            var root = node.Operation switch
            {
                OperationType.Query => schema.Query,
                OperationType.Mutation => schema.Mutation,
                OperationType.Subscription => schema.Subscription,
                _ => throw new ArgumentOutOfRangeException()
            };

            Types.Push(root);
        };
        LeaveOperationDefinition = node =>
        {
            Types.TryPop(out _);
        };

        EnterSelectionSet = node =>
        {
            ParentTypes.Push(CurrentType);
        };


        LeaveSelectionSet = node => { ParentTypes.TryPop(out _); };

        EnterFieldSelection = node =>
        {
            if (ParentType is not null)
            {
                var fieldDefinition = schema.GetField(ParentType.Name, node.Name);
                FieldDefinitions.Push(fieldDefinition ?? null);

                if (fieldDefinition?.Type is not null)
                {
                    var fieldTypeDefinition = Ast.UnwrapAndResolveType(schema, fieldDefinition.Type);

                    if (fieldTypeDefinition is not null && TypeIs.IsOutputType(fieldTypeDefinition))
                        Types.Push(fieldTypeDefinition);
                    else
                        Types.Push(null);
                }
                else
                {
                    Types.Push(null);
                }
            }
            else
            {
                Types.Push(null);
            }
        };
        LeaveFieldSelection = node =>
        {
            Types.TryPop(out _);
            FieldDefinitions.TryPop(out _);
        };

        EnterDirective = directive => { DirectiveDefinition = schema.GetDirectiveType(directive.Name); };
        LeaveDirective = node => { DirectiveDefinition = null; };

        EnterInlineFragment = node =>
        {
            var typeConditionAst = node.TypeCondition;
            if (typeConditionAst is not null)
            {
                var typeConditionDefinition = Ast.UnwrapAndResolveType(schema, typeConditionAst);

                if (typeConditionDefinition is not null)
                    Types.Push(TypeIs.IsOutputType(typeConditionDefinition) ? typeConditionDefinition : null);
                else
                    Types.Push(null);
            }
            else
            {
                Types.Push(CurrentType);
            }
        };
        LeaveInlineFragment = node => Types.TryPop(out _);

        EnterFragmentDefinition = node =>
        {
            var typeConditionAst = node.TypeCondition;
            var typeConditionDefinition = Ast.UnwrapAndResolveType(schema, typeConditionAst);

            if (typeConditionDefinition is not null)
                Types.Push(TypeIs.IsOutputType(typeConditionDefinition) ? typeConditionDefinition : null);
            else
                Types.Push(CurrentType);
        };
        LeaveFragmentDefinition = node => Types.TryPop(out _);

        EnterVariableDefinition = node =>
        {
            var inputTypeDefinition = Ast.UnwrapAndResolveType(schema, node.Type);

            if (inputTypeDefinition is not null)
                InputTypes.Push(TypeIs.IsInputType(inputTypeDefinition) ? inputTypeDefinition : null);
            else
                InputTypes.Push(null);
        };
        LeaveVariableDefinition = node => InputTypes.TryPop(out _);

        EnterArgument = node =>
        {
            // we're in directive
            if (DirectiveDefinition is not null)
            {
                if (DirectiveDefinition.TryGetArgument(node.Name, out var inputValueDefinition))
                {
                    ArgumentDefinition = inputValueDefinition;
                    DefaultValues.Push(inputValueDefinition.DefaultValue?.Value);

                    var argumentTypeDefinition = Ast.UnwrapAndResolveType(schema, inputValueDefinition.Type);

                    if (argumentTypeDefinition is not null)
                        InputTypes.Push(TypeIs.IsInputType(argumentTypeDefinition) ? argumentTypeDefinition : null);
                    else
                        InputTypes.Push(null);
                }
                else
                {
                    ArgumentDefinition = null;
                    DefaultValues.Push(null);
                    InputTypes.Push(null);
                }
            }
            else if (FieldDefinition is not null)
            {
                if (FieldDefinition.TryGetArgument(node.Name, out var inputValueDefinition))
                {
                    ArgumentDefinition = inputValueDefinition;
                    DefaultValues.Push(inputValueDefinition.DefaultValue?.Value);

                    var argumentTypeDefinition = Ast.UnwrapAndResolveType(schema, inputValueDefinition.Type);

                    if (argumentTypeDefinition is not null)
                        InputTypes.Push(TypeIs.IsInputType(argumentTypeDefinition) ? argumentTypeDefinition : null);
                    else
                        InputTypes.Push(null);
                }
                else
                {
                    ArgumentDefinition = null;
                    DefaultValues.Push(null);
                    InputTypes.Push(null);
                }
            }
            else
            {
                ArgumentDefinition = null;
                DefaultValues.Push(null);
                InputTypes.Push(null);
            }
        };
        LeaveArgument = node =>
        {
            ArgumentDefinition = null;
            DefaultValues.TryPop(out _);
            InputTypes.TryPop(out _);
        };
        
        EnterListValue = node =>
        {
            /*if (InputType is not null)
                InputTypes.Push(TypeIs.IsInputType(InputType) ? InputType : null);
            else
                InputTypes.Push(null);
            */

            // List positions never have a default value
            
            DefaultValues.Push(null);
        };
        LeaveListValue = node =>
        {
            InputTypes.TryPop(out _);
            DefaultValues.TryPop(out _);
        };

        EnterObjectField = node =>
        {
            if (InputType is InputObjectDefinition objectType)
            {
                var inputField = schema.GetInputField(objectType.Name, node.Name);

                if (inputField is not null)
                {
                    DefaultValues.Push(inputField.DefaultValue?.Value);

                    var inputFieldTypeDefinition = Ast.UnwrapAndResolveType(schema, inputField.Type);

                    if (inputFieldTypeDefinition is not null)
                        InputTypes.Push(TypeIs.IsInputType(inputFieldTypeDefinition) ? inputFieldTypeDefinition : null);
                    else
                        InputTypes.Push(null);
                }
                else
                {
                    DefaultValues.Push(null);
                    InputTypes.Push(null);
                }
            }
            else
            {
                DefaultValues.Push(null);
                InputTypes.Push(null);
            }
        };
        LeaveObjectField = node =>
        {
            DefaultValues.TryPop(out _);
            InputTypes.TryPop(out _);
        };
    }

    public InputValueDefinition? ArgumentDefinition { get; private set; }

    public TypeDefinition? CurrentType => Types.Count > 0 ? Types.Peek() : null;

    public ValueBase? DefaultValue => DefaultValues.Count > 0 ? DefaultValues.Peek() : null;

    public DirectiveDefinition? DirectiveDefinition { get; private set; }

    public FieldDefinition? FieldDefinition => FieldDefinitions.Count > 0 ? FieldDefinitions.Peek() : null;

    public TypeDefinition? InputType => InputTypes.Count > 0 ? InputTypes.Peek() : null;

    public TypeDefinition? ParentInputType
    {
        get
        {
            if (InputTypes.Count <= 1)
                return null;

            var currentType = InputTypes.Pop();
            var parentInputType = InputTypes.Peek();
            InputTypes.Push(currentType);
            return parentInputType;
        }
    }

    public TypeDefinition? ParentType => ParentTypes.Count > 0 ? ParentTypes.Peek() : null;

    protected Stack<ValueBase?> DefaultValues { get; } = new();

    protected Stack<FieldDefinition?> FieldDefinitions { get; } = new();

    protected Stack<TypeDefinition?> InputTypes { get; } = new();

    protected Stack<TypeDefinition?> ParentTypes { get; } = new();

    protected Stack<TypeDefinition?> Types { get; } = new();
}
using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation;

public class TypeTracker : RuleVisitor
{
    protected Stack<TypeDefinition?> ParentTypes { get; } = new();

    protected Stack<FieldDefinition?> FieldDefinitions { get; } = new();

    public TypeTracker(ISchema schema)
    {
        EnterOperationDefinition += node =>
        {
            var root = node.Operation switch
            {
                OperationType.Query => schema.Query,
                OperationType.Mutation => schema.Mutation,
                OperationType.Subscription => schema.Subscription,
                _ => throw new ArgumentOutOfRangeException()
            };

            ParentTypes.Push(root);
        };

        LeaveOperationDefinition += node =>
        {
            ParentTypes.TryPop(out _);
        };

        EnterFieldSelection += node =>
        {
            if (ParentType is not null)
            {
                var fieldDefinition = schema.GetField(ParentType.Name, node.Name);

                FieldDefinitions.Push(fieldDefinition ?? null);
            }
        };
    }

    public TypeDefinition? ParentType => ParentTypes.Count > 0 ? ParentTypes.Peek() : null;

    public FieldDefinition? FieldDefinition => FieldDefinitions.Count > 0 ? FieldDefinitions.Peek() : null;
}
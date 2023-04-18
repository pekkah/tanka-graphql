using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Executable;

public class FieldsWithSubscribers : Dictionary<FieldDefinition, Action<SubscriberBuilder>>
{
    public void Add(FieldDefinition fieldDefinition, Delegate subscriber)
    {
        if (!TryAdd(fieldDefinition, b => b.Run(subscriber)))
        {
            throw new InvalidOperationException($"{fieldDefinition} already has an subscriber");
        }
    }

    public void Add(FieldDefinition fieldDefinition, Subscriber subscriber)
    {
        if (!TryAdd(fieldDefinition, b => b.Run(subscriber)))
        {
            throw new InvalidOperationException($"{fieldDefinition} already has an subscriber");
        }
    }
}
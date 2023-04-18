using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Executable;

public class FieldsWithResolvers : Dictionary<FieldDefinition, Action<ResolverBuilder>>
{
    public void Add(FieldDefinition fieldDefinition, Delegate resolver)
    {
        if (!TryAdd(fieldDefinition, b => b.Run(resolver)))
        {
            throw new InvalidOperationException($"{fieldDefinition} already has an resolver");
        }
    }

    public void Add(FieldDefinition fieldDefinition, Resolver resolver)
    {
        if (!TryAdd(fieldDefinition, b => b.Run(resolver)))
        {
            throw new InvalidOperationException($"{fieldDefinition} already has an resolver");
        }
    }
}
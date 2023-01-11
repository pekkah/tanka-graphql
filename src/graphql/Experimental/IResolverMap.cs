using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental;

public interface IResolverMap
{
    Resolver? GetResolver(string typeName, string fieldName);

    IEnumerable<(string TypeName, IEnumerable<string> Fields)> GetTypes();
}

public static class ResolverMapExtensions
{
    public static Resolver? GetResolver(this IResolverMap map, ObjectDefinition type, FieldDefinition field)
    {
        return map.GetResolver(type.Name, field.Name);
    }
}
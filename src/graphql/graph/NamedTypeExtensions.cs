using System;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public static class NamedTypeExtensions
    {
        public static INamedType WithName(this INamedType namedType, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (namedType.Name == name) return namedType;

            //todo: move to interface
            if (namedType is ObjectType objectType)
            {
                return new ObjectType(
                    name,
                    new Fields(objectType.Fields),
                    objectType.Meta,
                    objectType.Interfaces);
            }

            throw new NotImplementedException("TODO: This should be part of the INamedType interface");
        }
    }
}
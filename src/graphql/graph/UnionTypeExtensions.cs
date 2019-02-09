using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public static class UnionTypeExtensions
    {
        public static UnionType WithEachPossibleType(
            this UnionType unionType,
            Func<ObjectType, ObjectType> withPossibleType)
        {
            var possibleTypes = unionType.PossibleTypes
                .Select(p => p.Value)
                .Select(withPossibleType)
                .Where(p => p != null);

            return unionType.WithPossibleTypes(
                possibleTypes
            );
        }

        public static UnionType WithPossibleTypes(
            this UnionType unionType, 
            IEnumerable<ObjectType> possibleTypes)
        {
            return new UnionType(
                unionType.Name,
                possibleTypes.ToList(),
                unionType.Meta);
        }
    }
}
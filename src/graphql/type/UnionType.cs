using System;
using System.Collections.Generic;

namespace fugu.graphql.type
{
    public class UnionType : IGraphQLType
    {
        public UnionType(string name, IEnumerable<IGraphQLType> possibleTypes, Meta meta = null)
        {
            Name = name;
            Meta = meta ?? new Meta();

            foreach (var possibleType in possibleTypes)
            {
                if (PossibleTypes.ContainsKey(possibleType.Name))
                    throw new InvalidOperationException(
                        $"Type {Name} already has possibleType with name {possibleType.Name}");

                PossibleTypes[possibleType.Name] = possibleType;
            }
        }

        public Dictionary<string, IGraphQLType> PossibleTypes { get; } = new Dictionary<string, IGraphQLType>();

        public string Name { get; }

        public bool IsPossible(IGraphQLType type)
        {
            return PossibleTypes.ContainsKey(type.Name);
        }

        public Meta Meta {get;}
    }
}
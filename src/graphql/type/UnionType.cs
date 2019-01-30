using System;
using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class UnionType : INamedType
    {
        public UnionType(string name, IEnumerable<ObjectType> possibleTypes, Meta meta = null)
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

        public Dictionary<string, ObjectType> PossibleTypes { get; } = new Dictionary<string, ObjectType>();

        public string Name { get; }

        public bool IsPossible(ObjectType type)
        {
            return PossibleTypes.ContainsKey(type.Name);
        }

        public Meta Meta {get;}
    }
}
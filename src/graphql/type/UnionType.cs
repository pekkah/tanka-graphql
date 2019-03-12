using System;
using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class UnionType : ComplexType, INamedType, IDescribable, IAbstractType
    {
        public UnionType(string name, IEnumerable<ObjectType> possibleTypes, string description = null,
            IEnumerable<DirectiveInstance> directives = null) : base(name, description, directives)
        {
            foreach (var possibleType in possibleTypes)
            {
                if (PossibleTypes.ContainsKey(possibleType.Name))
                    throw new InvalidOperationException(
                        $"Type {Name} already has possibleType with name {possibleType.Name}");

                PossibleTypes[possibleType.Name] = possibleType;
            }
        }

        public Dictionary<string, ObjectType> PossibleTypes { get; } = new Dictionary<string, ObjectType>();

        public bool IsPossible(ObjectType type)
        {
            return PossibleTypes.ContainsKey(type.Name);
        }
    }
}
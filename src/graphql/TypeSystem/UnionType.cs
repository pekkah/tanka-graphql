using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public class UnionType : INamedType, IDescribable, IAbstractType, IHasDirectives
    {
        private readonly DirectiveList _directives;

        public UnionType(string name, IEnumerable<ObjectType> possibleTypes, string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            Name = name;
            Description = description ?? string.Empty;
            _directives = new DirectiveList(directives);

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

        public string Description { get; }

        public IEnumerable<DirectiveInstance> Directives => _directives;

        public DirectiveInstance GetDirective(string name)
        {
            return _directives.GetDirective(name);
        }

        public bool HasDirective(string name)
        {
            return _directives.HasDirective(name);
        }

        public string Name { get; }
    }
}
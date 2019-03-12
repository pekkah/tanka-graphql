using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class InterfaceType : ComplexType, IHasDirectives, IDescribable, IAbstractType
    {
        public InterfaceType(string name, string description = null, IEnumerable<DirectiveInstance> directives = null)
            : base(name, description, directives)
        {
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public bool IsPossible(ObjectType type)
        {
            return type.Implements(this);
        }
    }
}
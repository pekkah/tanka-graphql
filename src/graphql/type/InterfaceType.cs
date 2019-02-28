using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class InterfaceType : ComplexType, IDirectives, IDescribable, IAbstractType
    {
        public InterfaceType(string name, Meta meta = null)
            : base(name)
        {
            Meta = meta ?? new Meta();
        }

        public Meta Meta { get; }

        public string Description => Meta.Description;

        public IEnumerable<DirectiveInstance> Directives => Meta.Directives;

        public DirectiveInstance GetDirective(string name)
        {
            return Meta.GetDirective(name);
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
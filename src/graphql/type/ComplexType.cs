using System.Collections.Generic;

namespace tanka.graphql.type
{
    public abstract class ComplexType : INamedType, IDescribable, IHasDirectives
    {
        private readonly DirectiveList _directives;

        protected ComplexType(string name, string description, IEnumerable<DirectiveInstance> directives)
        {
            Name = name;
            Description = description ?? string.Empty;
            _directives = new DirectiveList(directives);
        }

        public string Description { get; }

        public IEnumerable<DirectiveInstance> Directives => _directives;

        public DirectiveInstance GetDirective(string name)
        {
            return _directives.GetDirective(name);
        }

        public string Name { get; }
    }
}
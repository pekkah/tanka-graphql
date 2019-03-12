using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class InputObjectType : INamedType, IDescribable, IHasDirectives
    {
        private readonly DirectiveList _directives;

        public InputObjectType(string name, string description = null, IEnumerable<DirectiveInstance> directives = null)
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

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class EnumValue : IDescribable, IDeprecable, IHasDirectives
    {
        private readonly DirectiveList _directives;

        public EnumValue(string description, IEnumerable<DirectiveInstance> directives = null,
            string deprecationReason = null)
        {
            Description = description ?? string.Empty;
            DeprecationReason = deprecationReason;
            _directives = new DirectiveList(directives);
        }

        public string DeprecationReason { get; }

        public bool IsDeprecated => !string.IsNullOrEmpty(DeprecationReason);

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
    }
}
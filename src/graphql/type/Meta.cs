using System.Collections.Generic;

namespace fugu.graphql.type
{
    public class Meta : IDirectives
    {
        private readonly DirectiveContainer _directives;

        public Meta(string description = null, string deprecationReason = null, IEnumerable<DirectiveInstance> directives = null)
        {
            Description = description;
            DeprecationReason = deprecationReason;
            _directives = new DirectiveContainer(directives);
        }

        public Meta()
        {
            Description = string.Empty;
            DeprecationReason = null;
        }

        public string Description { get; }

        public string DeprecationReason { get; }

        public bool IsDeprecated => !string.IsNullOrEmpty(DeprecationReason);

        public IEnumerable<DirectiveInstance> Directives => _directives.Directives;

        public DirectiveInstance GetDirective(string name)
        {
            return _directives.GetDirective(name);
        }
    }
}
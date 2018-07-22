using System.Collections.Generic;

namespace fugu.graphql.type
{
    public class DirectiveContainer : IDirectives
    {
        private readonly Dictionary<string, DirectiveInstance>
            _directives = new Dictionary<string, DirectiveInstance>();

        public DirectiveContainer(IEnumerable<DirectiveInstance> directives)
        {
            if (directives != null)
                foreach (var directiveInstance in directives)
                    _directives[directiveInstance.Name] = directiveInstance;
        }

        public DirectiveContainer()
        {
        }

        public IEnumerable<DirectiveInstance> Directives => _directives.Values;

        public DirectiveInstance GetDirective(string name)
        {
            return _directives.ContainsKey(name) ? _directives[name] : null;
        }
    }
}
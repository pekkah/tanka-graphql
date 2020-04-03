using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public class DirectiveList : IHasDirectives, IEnumerable<DirectiveInstance>
    {
        private readonly Dictionary<string, DirectiveInstance>
            _directives = new Dictionary<string, DirectiveInstance>();

        public DirectiveList(IEnumerable<DirectiveInstance>? directives = null)
        {
            if (directives != null)
                foreach (var directiveInstance in directives)
                    _directives[directiveInstance.Name] = directiveInstance;
        }

        public IEnumerable<DirectiveInstance> Directives => _directives.Values;

        public DirectiveInstance? GetDirective(string name)
        {
            return _directives.ContainsKey(name) ? _directives[name] : null;
        }

        public IEnumerator<DirectiveInstance> GetEnumerator()
        {
            return _directives.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool HasDirective(string name)
        {
            return _directives.ContainsKey(name);
        }
    }
}
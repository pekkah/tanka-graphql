﻿using System.Collections;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem
{
    public class DirectiveList : IHasDirectives, IEnumerable<Directive>
    {
        private readonly Dictionary<string, Directive>
            _directives = new Dictionary<string, Directive>();

        public DirectiveList(IEnumerable<Directive>? directives = null)
        {
            if (directives != null)
                foreach (var directiveInstance in directives)
                    _directives[directiveInstance.Name] = directiveInstance;
        }

        public IEnumerable<Directive> Directives => _directives.Values;

        public Directive? GetDirective(string name)
        {
            return _directives.ContainsKey(name) ? _directives[name] : null;
        }

        public IEnumerator<Directive> GetEnumerator()
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
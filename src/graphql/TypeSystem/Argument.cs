using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public class Argument : IDescribable, IHasDirectives
    {
        private readonly DirectiveList _directives;

        public Argument(IType type, object defaultValue = null, string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            DefaultValue = defaultValue;
            Description = description ?? string.Empty;
            _directives = new DirectiveList(directives);
        }

        public IType Type { get; set; }

        public object DefaultValue { get; set; }

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

        [Obsolete]
        public static Argument Arg(IType type, object defaultValue = null, string description = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return new Argument(type, defaultValue, description);
        }
    }
}
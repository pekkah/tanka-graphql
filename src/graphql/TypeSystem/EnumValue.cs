using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public class EnumValue : IDescribable, IDeprecable, IHasDirectives
    {
        private readonly DirectiveList _directives;

        public EnumValue(
            string value,
            string? description = null, 
            IEnumerable<DirectiveInstance>? directives = null,
            string? deprecationReason = null)
        {
            Description = description ?? string.Empty;
            Value = value;
            DeprecationReason = deprecationReason;
            _directives = new DirectiveList(directives);
        }

        public string Value { get; }
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
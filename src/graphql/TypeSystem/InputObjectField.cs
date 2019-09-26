using System;
using System.Collections.Generic;
using Tanka.GraphQL.Execution;

namespace Tanka.GraphQL.TypeSystem
{
    public class InputObjectField : IHasDirectives, IDescribable
    {
        private readonly DirectiveList _directives;

        public InputObjectField(
            IType type,
            string description = null,
            object defaultValue = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            if (!TypeIs.IsInputType(type))
                throw new ArgumentOutOfRangeException(
                    $" Type '{type}' is not valid input type");

            Type = type;
            Description = description ?? string.Empty;
            DefaultValue = defaultValue;
            _directives = new DirectiveList(directives);
        }

        public object DefaultValue { get; set; }

        public IType Type { get; }

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
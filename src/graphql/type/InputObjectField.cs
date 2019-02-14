using System;
using System.Collections.Generic;
using tanka.graphql.execution;

namespace tanka.graphql.type
{
    public class InputObjectField : IDirectives, IDescribable
    {
        public InputObjectField(
            IType type,
            Meta meta = null,
            object defaultValue = null)
        {
            if (!TypeIs.IsInputType(type))
                throw new ArgumentOutOfRangeException(
                    $" Type '{type}' is not valid input type");

            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; set; }

        public Meta Meta { get; set; }

        public IType Type { get; }

        public string Description => Meta.Description;

        public IEnumerable<DirectiveInstance> Directives => Meta.Directives;

        public DirectiveInstance GetDirective(string name)
        {
            return Meta.GetDirective(name);
        }
    }
}
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class VariableDefinition
    {
        public readonly DefaultValue? DefaultValue;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly Location? Location;
        public readonly Type Type;
        public readonly Variable Variable;

        public VariableDefinition(
            Variable variable,
            Type type,
            DefaultValue? defaultValue,
            IReadOnlyCollection<Directive>? directives,
            in Location? location = default)
        {
            Variable = variable;
            Type = type;
            DefaultValue = defaultValue;
            Directives = directives;
            Location = location;
        }
    }
}
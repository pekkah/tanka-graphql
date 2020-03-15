using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public class VariableDefinition
    {
        public readonly Variable Variable;
        public readonly Type Type;
        public readonly DefaultValue? DefaultValue;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly Location Location;

        public VariableDefinition(
            in Variable variable,
            in Type type,
            in DefaultValue? defaultValue,
            in IReadOnlyCollection<Directive>? directives,
            in Location location)
        {
            Variable = variable;
            Type = type;
            DefaultValue = defaultValue;
            Directives = directives;
            Location = location;
        }
    }
}
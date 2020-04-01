using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class VariableDefinition: INode
    {
        public NodeKind Kind => NodeKind.VariableDefinition;
        public readonly DefaultValue? DefaultValue;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public Location? Location {get;}
        public readonly TypeBase Type;
        public readonly Variable Variable;

        public VariableDefinition(
            Variable variable,
            TypeBase type,
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
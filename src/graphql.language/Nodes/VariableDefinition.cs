using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Directives : CollectionNodeBase<Directive>
    {
        public Directives(
            IReadOnlyCollection<Directive> items, 
            in Location? location = default) 
            : base(items, in location)
        {
        }

        public override NodeKind Kind => NodeKind.Directives;
    }

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
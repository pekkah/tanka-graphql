namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class VariableDefinition : INode
    {
        public readonly DefaultValue? DefaultValue;
        public readonly Directives? Directives;
        public readonly TypeBase Type;
        public readonly Variable Variable;

        public VariableDefinition(
            Variable variable,
            TypeBase type,
            DefaultValue? defaultValue,
            Directives? directives,
            in Location? location = default)
        {
            Variable = variable;
            Type = type;
            DefaultValue = defaultValue;
            Directives = directives;
            Location = location;
        }

        public NodeKind Kind => NodeKind.VariableDefinition;
        public Location? Location { get; }
    }
}
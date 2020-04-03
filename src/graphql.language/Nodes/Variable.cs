namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Variable : Value, INode
    {
        public override NodeKind Kind => NodeKind.Variable;
        public override Location? Location {get;}
        public readonly Name Name;

        public Variable(
            in Name name,
            in Location? location = default)
        {
            Name = name;
            Location = location;
        }
    }
}
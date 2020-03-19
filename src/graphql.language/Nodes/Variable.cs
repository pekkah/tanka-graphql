namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Variable : IValue
    {
        public readonly Location? Location;
        public readonly Name Name;

        public Variable(
            Name name,
            in Location? location)
        {
            Name = name;
            Location = location;
        }
    }
}
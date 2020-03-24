namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Variable : Value
    {
        public readonly Location? Location;
        public readonly Name Name;

        public Variable(
            Name name,
            in Location? location = default)
        {
            Name = name;
            Location = location;
        }
    }
}
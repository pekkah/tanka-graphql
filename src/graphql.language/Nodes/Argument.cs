namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Argument
    {
        public readonly Location? Location;
        public readonly Name Name;
        public readonly Value Value;

        public Argument(
            Name name,
            Value value,
            in Location? location = default)
        {
            Name = name;
            Value = value;
            Location = location;
        }
    }
}
namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Argument
    {
        public readonly Name Name;
        public readonly IValue Value;
        public readonly Location Location;

        public Argument(
            Name name,
            IValue value,
            in Location location)
        {
            Name = name;
            Value = value;
            Location = location;
        }
    }
}
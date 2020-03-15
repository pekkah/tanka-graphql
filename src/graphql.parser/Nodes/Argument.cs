namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Argument
    {
        public readonly Name Name;
        public readonly IValue Value;
        public readonly Location Location;

        public Argument(
            in Name name,
            in IValue value,
            in Location location)
        {
            Name = name;
            Value = value;
            Location = location;
        }
    }
}
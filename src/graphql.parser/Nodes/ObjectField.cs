namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ObjectField
    {
        public readonly Name Name;
        public readonly IValue Value;
        public readonly Location Location;

        public ObjectField(
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
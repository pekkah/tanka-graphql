namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ObjectField
    {
        public readonly Name Name;
        public readonly IValue Value;
        public readonly Location Location;

        public ObjectField(
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
namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ObjectField
    {
        public readonly Location? Location;
        public readonly Name Name;
        public readonly Value Value;

        public ObjectField(
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
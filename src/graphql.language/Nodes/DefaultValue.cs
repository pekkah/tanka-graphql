namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class DefaultValue
    {
        public readonly Location? Location;
        public readonly Value Value;

        public DefaultValue(
            Value value,
            in Location? location = default)
        {
            Value = value;
            Location = location;
        }
    }
}
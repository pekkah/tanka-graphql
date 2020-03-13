namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class DefaultValue
    {
        public readonly IValue Value;
        public readonly Location? Location;

        public DefaultValue(
            in IValue value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}
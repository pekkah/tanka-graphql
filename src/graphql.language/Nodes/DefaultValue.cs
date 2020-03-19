namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class DefaultValue
    {
        public readonly Location? Location;
        public readonly IValue Value;

        public DefaultValue(
            IValue value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}
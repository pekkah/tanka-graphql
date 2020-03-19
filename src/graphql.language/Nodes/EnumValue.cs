namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class EnumValue : IValue
    {
        public readonly Location? Location;
        public readonly Name Value;

        public EnumValue(
            Name value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}
namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class EnumValue : Value
    {
        public readonly Location? Location;
        public readonly Name Value;

        public EnumValue(
            Name value,
            in Location? location = default)
        {
            Value = value;
            Location = location;
        }
    }
}
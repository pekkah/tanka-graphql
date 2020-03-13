namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class EnumValue : IValue
    {
        public readonly Name Value;
        public readonly Location? Location;

        public EnumValue(
            in Name value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }

    }
}
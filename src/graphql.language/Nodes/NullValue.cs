namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NullValue : Value
    {
        public readonly Location? Location;

        public NullValue(
            in Location? location = default)
        {
            Location = location;
        }
    }
}
namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NullValue : IValue
    {
        public readonly Location Location;

        public NullValue(
            in Location location)
        {
            Location = location;
        }
    }
}
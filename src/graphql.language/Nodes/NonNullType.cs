namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NonNullType : Type
    {
        public readonly Location? Location;
        public readonly Type OfType;

        public NonNullType(
            Type ofType,
            in Location? location)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
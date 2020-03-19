namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NonNullType : IType
    {
        public readonly Location? Location;
        public readonly IType OfType;

        public NonNullType(
            IType ofType,
            in Location? location)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
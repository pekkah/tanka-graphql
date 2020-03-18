namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NonNullType: IType
    {
        public readonly IType OfType;
        public readonly Location Location;

        public NonNullType(
            IType ofType,
            in Location location)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
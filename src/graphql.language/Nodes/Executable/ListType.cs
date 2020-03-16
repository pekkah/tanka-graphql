namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ListType: IType
    {
        public readonly IType OfType;
        public readonly Location Location;

        public ListType(
            IType ofType,
            in Location location)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
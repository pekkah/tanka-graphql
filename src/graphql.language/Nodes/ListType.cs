namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ListType : IType
    {
        public readonly Location? Location;
        public readonly IType OfType;

        public ListType(
            IType ofType,
            in Location? location)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
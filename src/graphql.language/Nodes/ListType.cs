namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ListType : Type
    {
        public readonly Location? Location;
        public readonly Type OfType;

        public ListType(
            Type ofType,
            in Location? location = default)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
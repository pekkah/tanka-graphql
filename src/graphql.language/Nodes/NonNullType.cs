namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NonNullType : TypeBase
    {
        public override NodeKind Kind => NodeKind.NonNullType;
        public override Location? Location {get;}
        public readonly TypeBase OfType;

        public NonNullType(
            TypeBase ofType,
            in Location? location = default)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
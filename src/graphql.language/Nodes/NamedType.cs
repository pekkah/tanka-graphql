namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NamedType : IType
    {
        public readonly Location? Location;
        public readonly Name Name;

        public NamedType(
            Name name,
            in Location? location)
        {
            Name = name;
            Location = location;
        }
    }
}
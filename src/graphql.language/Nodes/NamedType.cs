namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NamedType: IType
    {
        public readonly Name Name;
        public readonly Location Location;

        public NamedType(
            Name name,
            in Location location)
        {
            Name = name;
            Location = location;
        }
    }
}
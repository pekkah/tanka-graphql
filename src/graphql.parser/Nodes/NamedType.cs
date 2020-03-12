namespace Tanka.GraphQL.Language.Nodes
{
    public class NamedType: Type
    {
        public readonly Name Name;
        public readonly Location? Location;

        public NamedType(
            in Name name,
            in Location? location)
        {
            Name = name;
            Location = location;
        }
    }
}
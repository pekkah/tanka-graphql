namespace Tanka.GraphQL.Language.Nodes
{
    public class ListOf: Type
    {
        public readonly Type OfType;
        public readonly Location Location;

        public ListOf(
            in Type ofType,
            in Location location)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
namespace Tanka.GraphQL.Language.Nodes
{
    public class NonNullOf: Type
    {
        public readonly Type OfType;
        public readonly Location Location;

        public NonNullOf(
            Type ofType,
            in Location location)
        {
            OfType = ofType;
            Location = location;
        }
    }
}
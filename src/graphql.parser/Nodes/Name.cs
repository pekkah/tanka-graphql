namespace Tanka.GraphQL.Language.Nodes
{
    public readonly struct Name
    {
        public Name(in string value, in Location location)
        {
            Value = value;
            Location = location;
        }

        public readonly string Value;
        public readonly Location Location;
    }
}
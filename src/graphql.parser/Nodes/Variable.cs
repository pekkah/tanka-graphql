namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Variable: Value
    {
        public readonly Name Name;
        public readonly Location? Location;

        public Variable(
            in Name name,
            in Location? location)
        {
            Name = name;
            Location = location;
        }
    }
}
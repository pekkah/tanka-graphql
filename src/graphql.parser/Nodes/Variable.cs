namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Variable: IValue
    {
        public readonly Name Name;
        public readonly Location Location;

        public Variable(
            Name name,
            in Location location)
        {
            Name = name;
            Location = location;
        }
    }
}
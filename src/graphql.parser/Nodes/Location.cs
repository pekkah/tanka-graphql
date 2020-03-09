namespace Tanka.GraphQL.Language.Nodes
{
    public class Location
    {
        public Location(in int line, in int column)
        {
            Line = line;
            Column = column;
        }

        public readonly int Line;
        public readonly int Column;

        public override string ToString()
        {
            return $"@{Line}:{Column}";
        }
    }
}
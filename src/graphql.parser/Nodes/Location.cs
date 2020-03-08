namespace Tanka.GraphQL.Language.Nodes
{
    public readonly struct Location
    {
        public Location(in int line, in int column)
        {
            Line = line;
            Column = column;
        }

        public readonly int Line;
        public readonly int Column;
    }
}
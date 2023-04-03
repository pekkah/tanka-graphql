namespace Tanka.GraphQL.Language.Nodes;

public readonly struct Location
{
    public Location(int line, int column)
    {
        Line = line;
        Column = column;
    }

    public readonly int Line;
    public readonly int Column;

    public override string ToString()
    {
        if (Equals(default(Location)))
            return "@";

        return $"@{Line}:{Column}";
    }
}
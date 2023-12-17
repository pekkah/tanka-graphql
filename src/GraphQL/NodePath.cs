namespace Tanka.GraphQL;

/// <summary>
///     Tracks the path of a node in the GraphQL document.
/// </summary>
public class NodePath
{
    private readonly List<object> _path = new();

    public NodePath()
    {
    }

    protected NodePath(object[] segments)
    {
        _path.AddRange(segments);
    }

    public IEnumerable<object> Segments => _path;

    public NodePath Append(string fieldName)
    {
        _path.Add(fieldName);
        return this;
    }

    public NodePath Append(int index)
    {
        _path.Add(index);
        return this;
    }

    public NodePath Fork()
    {
        return new(Segments.ToArray());
    }
}
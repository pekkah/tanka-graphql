namespace Tanka.GraphQL;

public class QueryException : Exception
{
    public QueryException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }

    public required NodePath Path { get; set; }
}
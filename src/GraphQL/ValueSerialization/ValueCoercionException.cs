using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.ValueSerialization;

public class ValueCoercionException : Exception
{
    public ValueCoercionException(string message, object? value, INode type)
        : base(message)
    {
        Type = type;
        Value = value;
    }

    public INode Type { get; }

    public object? Value { get; set; }
}
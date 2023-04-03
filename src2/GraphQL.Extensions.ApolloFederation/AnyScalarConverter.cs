using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public class AnyScalarConverter : IValueConverter
{
    public object? Serialize(object value)
    {
        return value;
    }

    public ValueBase SerializeLiteral(object? value)
    {
        return new NullValue();
    }

    public object? ParseValue(object input)
    {
        return input;
    }

    public object? ParseLiteral(ValueBase input)
    {
        return input;
    }
}
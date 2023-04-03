namespace Tanka.GraphQL.Features;

public interface ICoercedVariableValuesFeature
{
    IReadOnlyDictionary<string, object?> CoercedVariableValues { get; set; }
}
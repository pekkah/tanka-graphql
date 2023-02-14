namespace Tanka.GraphQL.Features;

public class CoercedVariableValuesFeature : ICoercedVariableValuesFeature
{
    public CoercedVariableValuesFeature()
    {
        CoercedVariableValues = Empty;
    }

    private static IReadOnlyDictionary<string, object?> Empty { get; } = new Dictionary<string, object?>();

    public IReadOnlyDictionary<string, object?> CoercedVariableValues { get; set; }
}
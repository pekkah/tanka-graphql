using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental.Features;

public interface ICoercedVariableValuesFeature
{
    IReadOnlyDictionary<string, object?> CoercedVariableValues { get; set; }
}
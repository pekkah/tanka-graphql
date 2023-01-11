﻿using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental.Features;

public class CoercedVariableValuesFeature : ICoercedVariableValuesFeature
{
    public CoercedVariableValuesFeature()
    {
        CoercedVariableValues = Empty;
    }

    private static IReadOnlyDictionary<string, object?> Empty { get; } = new Dictionary<string, object?>();

    public required IReadOnlyDictionary<string, object?> CoercedVariableValues { get; set; }
}
﻿using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

internal class EmptySchema : ExecutableSchema
{
    private static readonly ObjectDefinition EmptyQuery = """
        type Query 
        {
        }
        """;

    private static readonly IReadOnlyDictionary<string, IValueConverter> ScalarSerializers =
        new Dictionary<string, IValueConverter>(0);

    private static readonly IReadOnlyDictionary<string, DirectiveDefinition> DirectiveTypes =
        new Dictionary<string, DirectiveDefinition>(0);

    private static readonly IReadOnlyDictionary<string, Dictionary<string, InputValueDefinition>> InputFields =
        new Dictionary<string, Dictionary<string, InputValueDefinition>>(0);

    private static readonly IReadOnlyDictionary<string, Dictionary<string, FieldDefinition>> Fields =
        new Dictionary<string, Dictionary<string, FieldDefinition>>(0);

    public EmptySchema() : base(
        Types,
        Fields,
        InputFields,
        DirectiveTypes,
        EmptyQuery,
        ResolversMap.None,
        ScalarSerializers)
    {
    }

    private static Dictionary<string, TypeDefinition> Types => new(1)
    {
        ["Query"] = EmptyQuery
    };
}
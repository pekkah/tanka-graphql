using System;
using Tanka.GraphQL.Introspection;

namespace Tanka.GraphQL.Server.Links;

public static class IntrospectionSchemaBuilderExtensions
{
    public static SchemaBuilder AddIntrospectedSchema(
        this SchemaBuilder builder,
        string introspectionExecutionResultJson)
    {
        if (string.IsNullOrWhiteSpace(introspectionExecutionResultJson))
            throw new ArgumentNullException(nameof(introspectionExecutionResultJson));

        var result = IntrospectionParser.Deserialize(introspectionExecutionResultJson);
        builder.AddIntrospectedSchema(result.Schema);

        return builder;
    }
}
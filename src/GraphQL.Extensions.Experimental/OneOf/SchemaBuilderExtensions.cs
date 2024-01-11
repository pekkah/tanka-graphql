using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Extensions.Experimental.OneOf;

public static class SchemaBuilderExtensions
{
    public static SchemaBuilder AddOneOf(this SchemaBuilder builder)
    {
        builder.Add(OneOfDirective.Directive);
        return builder;
    }
}
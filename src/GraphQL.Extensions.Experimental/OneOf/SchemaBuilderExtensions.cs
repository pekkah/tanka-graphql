using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Extensions.Experimental.OneOf;

public static class SchemaBuilderExtensions
{
    /// <summary>
    ///     Add support for the @oneOf directive.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static SchemaBuilder AddOneOf(this SchemaBuilder builder)
    {
        builder.Add(OneOfDirective.Directive);
        return builder;
    }
}
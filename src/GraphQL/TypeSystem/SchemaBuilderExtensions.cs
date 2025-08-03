using Tanka.GraphQL.Executable;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Extension methods for SchemaBuilder to simplify adding common directive definitions
/// </summary>
public static class SchemaBuilderExtensions
{
    /// <summary>
    /// Adds the @defer directive definition to enable incremental delivery
    /// </summary>
    public static SchemaBuilder AddDeferDirective(this SchemaBuilder builder)
    {
        return builder.Add(@"
            directive @defer(
                if: Boolean! = true
                label: String
            ) on FRAGMENT_SPREAD | INLINE_FRAGMENT
        ");
    }

    /// <summary>
    /// Adds the @stream directive definition to enable streaming of list fields
    /// </summary>
    public static SchemaBuilder AddStreamDirective(this SchemaBuilder builder)
    {
        return builder.Add(@"
            directive @stream(
                if: Boolean! = true
                label: String
                initialCount: Int! = 0
            ) on FIELD
        ");
    }

    /// <summary>
    /// Adds both @defer and @stream directive definitions for incremental delivery
    /// </summary>
    public static SchemaBuilder AddIncrementalDeliveryDirectives(this SchemaBuilder builder)
    {
        return builder
            .AddDeferDirective()
            .AddStreamDirective();
    }

    // Extension methods for ExecutableSchemaBuilder that delegate to its Schema property

    /// <summary>
    /// Adds the @defer directive definition to enable incremental delivery
    /// </summary>
    public static ExecutableSchemaBuilder AddDeferDirective(this ExecutableSchemaBuilder builder)
    {
        builder.Schema.AddDeferDirective();
        return builder;
    }

    /// <summary>
    /// Adds the @stream directive definition to enable streaming of list fields
    /// </summary>
    public static ExecutableSchemaBuilder AddStreamDirective(this ExecutableSchemaBuilder builder)
    {
        builder.Schema.AddStreamDirective();
        return builder;
    }

    /// <summary>
    /// Adds both @defer and @stream directive definitions for incremental delivery
    /// </summary>
    public static ExecutableSchemaBuilder AddIncrementalDeliveryDirectives(this ExecutableSchemaBuilder builder)
    {
        builder.Schema.AddIncrementalDeliveryDirectives();
        return builder;
    }
}
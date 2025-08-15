using System.Collections.Generic;

using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

/// <summary>
/// Extension methods for configuring Apollo Federation in the schema build pipeline
/// </summary>
public static class SchemaBuildOptionsExtensions
{
    /// <summary>
    /// Configure Apollo Federation subgraph with the specified options
    /// </summary>
    public static SchemaBuildOptions UseFederation(this SchemaBuildOptions options, SubgraphOptions federationOptions)
    {
        // Add Federation value converters
        options.ValueConverters ??= new Dictionary<string, IValueConverter>();
        options.ValueConverters["_Any"] = new AnyScalarConverter();
        options.ValueConverters["FieldSet"] = new FieldSetScalarConverter();

        // Set composite schema loader with Federation support and HTTP fallback
        // This allows Federation URLs to work while still supporting regular HTTP schema loading
        options.SchemaLoader = new CompositeSchemaLoader(
            new FederationSchemaLoader(),
            new HttpSchemaLoader()
        );

        // Add Federation initialization middleware to Initialization stage
        // This adds the @link directive BEFORE LinkProcessingMiddleware runs
        options.Use(SchemaBuildStage.Initialization, async (context, next) =>
        {
            var middleware = new FederationInitializationMiddleware(federationOptions);
            return await middleware.InvokeAsync(context, next);
        });

        // Add Federation configuration middleware to TypeResolution stage  
        // This configures resolvers AFTER Federation types have been imported via @link
        options.Use(SchemaBuildStage.TypeResolution, async (context, next) =>
        {
            var middleware = new FederationConfigurationMiddleware(federationOptions);
            return await middleware.InvokeAsync(context, next);
        });

        return options;
    }
}
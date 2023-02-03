namespace Tanka.GraphQL.Extensions.ApolloFederation;

public static class FederationExecutableSchemaBuilderExtensions
{
    public static ExecutableSchemaBuilder AddSubgraph(
        this ExecutableSchemaBuilder builder,
        SubgraphOptions options)
    {
        builder.AddConfiguration(new SubgraphConfiguration(options));
        builder.AddValueConverter("_Any", new AnyScalarConverter());
        builder.AddValueConverter("_FieldSet", new FieldSetScalarConverter());

        return builder;
    }
}
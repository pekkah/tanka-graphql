using Tanka.GraphQL.Executable;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public static class FederationExecutableSchemaBuilderExtensions
{
    public static ExecutableSchemaBuilder AddSubgraph(
        this ExecutableSchemaBuilder builder,
        SubgraphOptions options)
    {
        builder.Add(new SubgraphConfiguration(options));
        builder.Add("_Any", new AnyScalarConverter());
        builder.Add("_FieldSet", new FieldSetScalarConverter());

        return builder;
    }
}
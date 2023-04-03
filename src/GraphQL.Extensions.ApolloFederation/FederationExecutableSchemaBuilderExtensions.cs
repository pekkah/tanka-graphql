using Tanka.GraphQL.Executable;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public static class FederationExecutableSchemaBuilderExtensions
{
    public static ExecutableSchemaBuilder AddSubgraph(
        this ExecutableSchemaBuilder builder,
        SubgraphOptions options)
    {
        builder.Add(new SubgraphConfiguration(options));
        builder.AddConverter("_Any", new AnyScalarConverter());
        builder.AddConverter("_FieldSet", new FieldSetScalarConverter());

        return builder;
    }
}
using Tanka.GraphQL.SchemaBuilding;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public static class FederationSchemaBuilderExtensions
    {
        public static SchemaBuilder AddFederationDirectives(this SchemaBuilder builder)
        {
            builder.Include(FederationTypes._FieldSet);
            builder.Include(FederationTypes._FieldSet.Name, new FieldSetScalarConverter());
            builder.Include(FederationTypes._Any);
            builder.Include(FederationTypes._Any.Name, new AnyScalarConverter());

            builder.Include(FederationTypes.External);
            builder.Include(FederationTypes.Requires);
            builder.Include(FederationTypes.Provides);
            builder.Include(FederationTypes.Key);
            builder.Include(FederationTypes.Extends);

            return builder;
        }
    }
}
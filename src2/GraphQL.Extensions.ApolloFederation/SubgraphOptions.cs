namespace Tanka.GraphQL.Extensions.ApolloFederation;

public record SubgraphOptions(IReferenceResolversMap ReferenceResolvers)
{
    public static SubgraphOptions Default = new(new DictionaryReferenceResolversMap());
}
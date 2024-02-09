namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

[UsesVerify]
public class InterfaceGeneratorFacts
{
    [Fact]
    public Task Generate_fields_from_properties()
    {
        var source = """
                     using Tanka.GraphQL.Server;

                     namespace Tests;

                     [InterfaceType]
                     public partial interface IAnimal   
                     {
                        string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task Generate_with_graphql_name()
    {
        var source = """
                     using Tanka.GraphQL.Server;

                     namespace Tests;

                     [InterfaceType]
                     [GraphQLName("Animal")]
                     public partial interface IAnimal
                     {
                        string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}
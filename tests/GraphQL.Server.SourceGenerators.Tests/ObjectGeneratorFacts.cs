namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

[UsesVerify]
public class ObjectGeneratorFacts
{
    [Fact]
    public Task StaticClass_Generate_method_resolver()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static int Id(int? p1) => 123;
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task Generate_ObjectType_type_name()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Person Person(int id) = new Person();
                     }

                     [ObjectType]
                     public class Person 
                     {
                        public string Name { get; set;}
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task StaticClass_Generate_property_resolver()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string Id { get; set;}
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}
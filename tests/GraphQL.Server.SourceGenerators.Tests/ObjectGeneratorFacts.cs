namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

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
    public Task StaticClass_Generate_method_subscriber()
    {
        var source = """
                     using Tanka.GraphQL.Server;

                     namespace Tests;

                     [ObjectType]
                     public static class Subscription
                     {
                        public static IAsyncEnumerable<int> Random(int from, int to, CancellationToken cancellationToken)
                        {
                            foreach(var i in Enumerable.Range(from, to))
                            {
                                yield return i;
                                await Task.Delay(i*100);
                            }
                        }
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
    public Task Generate_ObjectType_type_name_no_namespace()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                    
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

    [Fact]
    public Task HelloWorld()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;
                     
                     /// <summary>
                     ///     Root query type by naming convention
                     ///     <remarks>
                     ///         We define it as static class so that the generator does not try
                     ///         to use the initialValue as the source of it.
                     ///     </remarks>
                     /// </summary>
                     [ObjectType]
                     public static class Query
                     {
                         public static World World() => new();
                     }

                     [ObjectType]
                     public class World
                     {
                         /// <summary>
                         ///     Simple field with one string argument and string return type
                         /// </summary>
                         /// <param name="name">name: String!</param>
                         /// <returns>String!</returns>
                         public string Hello(string name) => $"Hello {name}";
                     
                         /// <summary>
                         ///     This is the async version of the Hello method
                         /// </summary>
                         /// <param name="name"></param>
                         /// <returns></returns>
                         public async Task<string> HelloAsync(string name) => await Task.FromResult($"Hello {name}");
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task Random_AsyncEnumerable()
    {
        var source = """
                     using Tanka.GraphQL.Server;

                     namespace Tests;

                     [ObjectType]
                     public class World
                     {
                         /// <summary>
                         ///     This is subscription field producing random integers of count between from and to
                         /// </summary>
                         /// <returns></returns>
                         public async IAsyncEnumerable<int> Random(int from, int to, int count, [EnumeratorCancellation] CancellationToken cancellationToken)
                         {
                             var r = new Random();
                     
                             for (var i = 0; i < count; i++)
                             {
                                 yield return r.Next(from, to);
                                 cancellationToken.ThrowIfCancellationRequested();
                                 await Task.Delay(i * 10, cancellationToken);
                             }
                         }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task Implemented_types()
    {
        var source = """
                     using Tanka.GraphQL.Server;

                     namespace Tests;
                     
                     [InterfaceType]
                     public partial interface IValue
                     {
                         public string Hello { get; }
                     }
                     
                     [ObjectType]
                     public partial class StringValue : IValue
                     {
                         public required string Value { get; init; }
                         
                         public string Hello => $"Hello from {__Typename}";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}
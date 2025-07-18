namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public class InterfaceTypeParserErrorsFacts
{
    [Fact]
    public Task InvalidAttributeCombination_InterfaceTypeAndObjectType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     [ObjectType]
                     public interface IAnimal
                     {
                        string Name { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InvalidAttributeCombination_InterfaceTypeAndInputType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     [InputType]
                     public interface IAnimal
                     {
                        string Name { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MalformedSyntax_MissingBraces()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MalformedSyntax_MissingPropertyType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        Name { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MalformedSyntax_InvalidPropertyName()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string 123InvalidName { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MalformedSyntax_InvalidMethodName()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string 123InvalidMethod();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task EmptyInterface()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithStaticMembers()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        static string StaticProperty { get; set; }
                        static string StaticMethod() => "static";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithDefaultImplementations()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        string Description => $"Animal named {Name}";
                        string GetInfo() => $"Info for {Name}";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithVoidMethods()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        void VoidMethod();
                        Task VoidTask();
                        ValueTask VoidValueTask();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithComplexGenericTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        Dictionary<string, List<object>> ComplexProperty { get; }
                        Task<Dictionary<string, List<object>>> ComplexMethod();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithNullableTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string? Name { get; }
                        string?[] NullableArray { get; }
                        List<string?> NullableList { get; }
                        string? GetNullableName();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithAsyncMethods()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        Task<string> GetNameAsync();
                        ValueTask<string> GetNameValueAsync();
                        IAsyncEnumerable<string> GetNamesAsync();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithParameterizedMethods()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        string GetInfo(string format);
                        string GetInfoWithContext(ResolverContext context);
                        string GetInfoWithCancellation(CancellationToken cancellationToken);
                        string GetInfoWithService(IServiceProvider serviceProvider);
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithAttributedParameters()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        string GetInfo([FromArguments] string format);
                        string GetService([FromServices] IService service);
                     }
                     
                     public interface IService
                     {
                        string GetData();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithInvalidGraphQLName()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     [GraphQLName("123InvalidName")]
                     public interface IAnimal
                     {
                        string Name { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithUnknownReturnType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        UnknownType GetUnknown();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithUnknownParameterType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        string GetInfo(UnknownType parameter);
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithNestedInterfaces()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        IOwner Owner { get; }
                     }
                     
                     [InterfaceType]
                     public interface IOwner
                     {
                        string Name { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithCircularReference()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                        IAnimal Parent { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithMissingUsingStatements()
    {
        var source = """
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithInheritance()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal : ILivingThing
                     {
                        string Name { get; }
                     }
                     
                     public interface ILivingThing
                     {
                        bool IsAlive { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InterfaceWithMultipleInheritance()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IAnimal : ILivingThing, INamed
                     {
                        string Species { get; }
                     }
                     
                     public interface ILivingThing
                     {
                        bool IsAlive { get; }
                     }
                     
                     public interface INamed
                     {
                        string Name { get; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}
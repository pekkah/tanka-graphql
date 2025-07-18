namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public class ObjectTypeParserErrorsFacts
{
    [Fact]
    public Task InvalidAttributeCombination_ObjectTypeAndInputType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     [InputType]
                     public static class Query
                     {
                        public static string Hello() => "world";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InvalidAttributeCombination_ObjectTypeAndInterfaceType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     [InterfaceType]
                     public static class Query
                     {
                        public static string Hello() => "world";
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

                     [ObjectType]
                     public static class Query
                     {
                        public static string Hello() => "world"
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MalformedSyntax_MissingReturnType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Hello() => "world";
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

                     [ObjectType]
                     public static class Query
                     {
                        public static string 123InvalidName() => "world";
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

                     [ObjectType]
                     public static class Query
                     {
                        public static string 123Invalid { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task NonPublicMembersIgnored()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string PublicMethod() => "world";
                        private static string PrivateMethod() => "hidden";
                        protected static string ProtectedMethod() => "hidden";
                        internal static string InternalMethod() => "hidden";
                        
                        public static string PublicProperty { get; set; }
                        private static string PrivateProperty { get; set; }
                        protected static string ProtectedProperty { get; set; }
                        internal static string InternalProperty { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task VoidMethodsHandled()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static void VoidMethod() { }
                        public static Task VoidTask() => Task.CompletedTask;
                        public static ValueTask VoidValueTask() => ValueTask.CompletedTask;
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task NullableReferenceTypesHandled()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string? NullableString() => null;
                        public static string?[] NullableStringArray() => new string?[] { null, "test" };
                        public static List<string?> NullableStringList() => new List<string?> { null, "test" };
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MissingUsingStatements()
    {
        var source = """
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string Hello() => "world";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task EmptyClass()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexParameterWithoutAttribute()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string Hello(Person person) => $"Hello {person.Name}";
                     }
                     
                     public class Person
                     {
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task UnknownReturnType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static UnknownType Hello() => new UnknownType();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InheritanceWithoutInterfaceType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public class Query : BaseQuery
                     {
                        public string Hello() => "world";
                     }
                     
                     public class BaseQuery
                     {
                        public string BaseMethod() => "base";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task NestedGenericTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Dictionary<string, List<Person>> ComplexReturn() => new();
                        public static Task<Dictionary<string, List<Person?>>> ComplexAsyncReturn() => Task.FromResult(new Dictionary<string, List<Person?>>());
                     }
                     
                     public class Person
                     {
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InvalidGraphQLName()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     [GraphQLName("123InvalidName")]
                     public static class Query
                     {
                        public static string Hello() => "world";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ConflictingMemberNames()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string Hello() => "method";
                        public static string Hello { get; set; } = "property";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task StaticAndInstanceMembersMixed()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public class Query
                     {
                        public static string StaticMethod() => "static";
                        public string InstanceMethod() => "instance";
                        
                        public static string StaticProperty { get; set; } = "static";
                        public string InstanceProperty { get; set; } = "instance";
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}
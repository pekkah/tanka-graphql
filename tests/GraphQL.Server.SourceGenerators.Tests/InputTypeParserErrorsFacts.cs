namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public class InputTypeParserErrorsFacts
{
    [Fact]
    public Task InvalidAttributeCombination_InputTypeAndObjectType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     [ObjectType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InvalidAttributeCombination_InputTypeAndInterfaceType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     [InterfaceType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
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

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MalformedSyntax_MissingPropertyType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public Content { get; set; }
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

                     [InputType]
                     public class InputMessage
                     {
                        public string 123InvalidName { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithMethods()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                        public string GetContent() => Content;
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task NonPublicPropertiesIgnored()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string PublicProperty { get; set; }
                        private string PrivateProperty { get; set; }
                        protected string ProtectedProperty { get; set; }
                        internal string InternalProperty { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task EmptyInputType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task CircularReferenceInInputType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                        public InputMessage Parent { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexNestedInputTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                        public NestedType Nested { get; set; }
                        public List<NestedType> NestedList { get; set; }
                     }
                     
                     public class NestedType
                     {
                        public string Value { get; set; }
                        public Dictionary<string, object> Metadata { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithNullableCollections()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public List<string?> NullableStringList { get; set; }
                        public string?[] NullableStringArray { get; set; }
                        public Dictionary<string, object?> NullableValueDict { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithReadOnlyProperties()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                        public string ReadOnlyProperty { get; }
                        public string WriteOnlyProperty { set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithStaticProperties()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                        public static string StaticProperty { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithInvalidGraphQLName()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     [GraphQLName("123InvalidName")]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithUnknownPropertyType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                        public UnknownType Unknown { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithComplexGenerics()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage<T> where T : class
                     {
                        public string Content { get; set; }
                        public T GenericProperty { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithNestedGenerics()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public Dictionary<string, List<Dictionary<string, object>>> ComplexNested { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithTaskProperties()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                        public Task<string> AsyncContent { get; set; }
                        public ValueTask<int> AsyncValue { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithEnumProperties()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                        public MessageType Type { get; set; }
                        public MessageType? OptionalType { get; set; }
                     }
                     
                     public enum MessageType
                     {
                        Text,
                        Image,
                        Video
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task InputTypeWithMissingUsingStatements()
    {
        var source = """
                     namespace Tests;

                     [InputType]
                     public class InputMessage
                     {
                        public string Content { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}
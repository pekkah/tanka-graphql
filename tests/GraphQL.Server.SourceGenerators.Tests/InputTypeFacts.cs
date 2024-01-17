namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

[UsesVerify]
public class InputTypeFacts
{
    [Fact]
    public Task Generate_InputType()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                    
                     [InputType]
                     public class InputMessage
                     {
                        public string Id { get; set; }
                        public string Content { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task Generate_InputType_with_nullable_string()
    {
        var source = """
                     using Tanka.GraphQL.Server;

                     [InputType]
                     public class InputMessage
                     {
                        public string? Content { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task Generate_InputType_with_nullable_int()
    {
        var source = """
                     using Tanka.GraphQL.Server;

                     [InputType]
                     public class InputMessage
                     {
                        public int? Content { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task Generate_InputType_with_listof_nullable_class()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     using System.Collections.Generic;
                     
                     [InputType]
                     public class InputMessage
                     {
                        public List<Person?> Content { get; set; }
                     }
                     
                     public class Person 
                     {
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task Generate_InputType_with_complex_property()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     using System.Collections.Generic;

                     [InputType]
                     public class InputMessage
                     {
                        public Person Content { get; set; }
                     }

                     public class Person
                     {
                        public int Id { get;set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}
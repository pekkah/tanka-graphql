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

}
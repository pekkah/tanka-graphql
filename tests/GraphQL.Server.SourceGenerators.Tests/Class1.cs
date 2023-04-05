namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

[UsesVerify]
public class EnumGeneratorSnapshotTests
{
    [Fact]
    public Task Generate_model()
    {
        var source = """
                     using Tanka.GraphQL.Language.Nodes;

                     """;

        //return TestHelper.Verify(source);
        return Task.CompletedTask;
    }
}
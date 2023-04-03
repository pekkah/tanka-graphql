using System.IO;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.RealWorldSchemas;

public class ParseGitHubSchemaFacts
{
    public ParseGitHubSchemaFacts()
    {
        GitHubBytes = File.ReadAllBytes("RealWorldSchemas/github.graphql");
    }

    public byte[] GitHubBytes { get; }

    [Fact]
    public void Parse()
    {
        var parser = Parser.Create(GitHubBytes);
        var typeSystem = parser.ParseTypeSystemDocument();

        Assert.NotNull(typeSystem.TypeDefinitions);
        Assert.NotEmpty(typeSystem.TypeDefinitions);
    }

    [Fact]
    public void ParseAndPrintAndParse()
    {
        var parser = Parser.Create(GitHubBytes);
        var typeSystem = parser.ParseTypeSystemDocument();

        var sdl = Printer.Print(typeSystem);

        var parser2 = Parser.Create(sdl);
        var typeSystem2 = parser2.ParseTypeSystemDocument();

        Assert.NotNull(typeSystem2.TypeDefinitions);
        Assert.NotEmpty(typeSystem2.TypeDefinitions);
    }
}
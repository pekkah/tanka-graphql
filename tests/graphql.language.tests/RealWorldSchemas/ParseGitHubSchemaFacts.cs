using System.IO;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.RealWorldSchemas
{
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
    }
}
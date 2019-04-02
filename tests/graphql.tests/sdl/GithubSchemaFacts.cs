using System;
using System.IO;
using System.Reflection;
using System.Text;
using tanka.graphql.sdl;
using Xunit;

namespace tanka.graphql.tests.sdl
{
    public class GithubSchemaFacts
    {
        private static string GetGitHubSchema()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("tanka.graphql.tests.github.graphql");
            using (var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact(Skip = "Parser does not yet support descriptions in sdl")]
        public void LoadSchema()
        {
            /* Given */
            var sdl = GetGitHubSchema();

            /* When */
            var schema = Sdl.Schema(Parser.ParseDocument(sdl));

            /* Then */
            Assert.NotNull(schema);
            Assert.NotNull(schema.Query);
        }
    }
}
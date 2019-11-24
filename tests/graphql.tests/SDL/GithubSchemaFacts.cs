using System;
using System.IO;
using System.Reflection;
using System.Text;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Xunit;

namespace Tanka.GraphQL.Tests.SDL
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
            var schema = new SchemaBuilder().Sdl(Parser.ParseDocument(sdl))
                .Build();

            /* Then */
            Assert.NotNull(schema);
            Assert.NotNull(schema.Query);
        }
    }
}
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Tests.SDL;

public class GithubSchemaFacts
{
    [Fact]
    public async Task LoadSchema()
    {
        /* Given */
        var sdl = GetGitHubSchema();

        /* When */
        var schema = await new SchemaBuilder()
            .Add(sdl)
            .Build();

        /* Then */
        Assert.NotNull(schema);
        Assert.NotNull(schema.Query);
    }

    private static string GetGitHubSchema()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream("Tanka.GraphQL.Tests.github.graphql");
        using (var reader =
               new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }
}
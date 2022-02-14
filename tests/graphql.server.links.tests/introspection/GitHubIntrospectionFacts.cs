using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Server.Links.Tests.Introspection;

public class GitHubIntrospectionFacts
{
    [Fact]
    public async Task Read_types()
    {
        /* Given */
        var builder = new SchemaBuilder();
        /*builder.Scalar("URI", out _, new StringConverter());
        builder.Scalar("DateTime", out _, new StringConverter());
        builder.Scalar("Date", out _, new StringConverter());
        builder.Scalar("HTML", out _, new StringConverter());
        builder.Scalar("X509Certificate", out _, new StringConverter());
        builder.Scalar("GitObjectID", out _, new StringConverter());
        builder.Scalar("GitTimestamp", out _, new StringConverter());
        builder.Scalar("GitSSHRemote", out _, new StringConverter());*/

        /* When */
        var schema = await builder.AddIntrospectedSchema(GetGitHubSchema())
            .Build(new SchemaBuildOptions());

        /* Then */
        Assert.True(schema is not null);
    }


    private static string GetGitHubSchema()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream =
            assembly.GetManifestResourceStream("Tanka.GraphQL.Server.Links.Tests.github.introspection");
        using (var reader =
               new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }
}
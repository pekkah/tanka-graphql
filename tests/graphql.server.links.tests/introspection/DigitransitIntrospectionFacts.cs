using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Server.Links.Tests.Introspection;

public class DigitransitIntrospectionFacts
{
    [Fact]
    public async Task Read_types()
    {
        /* Given */
        var builder = new SchemaBuilder();
        builder.Add(@"
scalar Long
scalar Lat
scalar Polyline
");

        /* When */
        var schema = await builder.AddIntrospectedSchema(GetDigitransitIntrospection())
            .Build(new SchemaBuildOptions());

        /* Then */
        Assert.True(schema.GetNamedType("Query") is not null);
    }

    private static string GetDigitransitIntrospection()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream =
            assembly.GetManifestResourceStream("Tanka.GraphQL.Server.Links.Tests.digitransit.introspection");
        using (var reader =
               new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }
}
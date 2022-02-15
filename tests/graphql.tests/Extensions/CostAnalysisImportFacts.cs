using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Extensions;

public class CostAnalysisImportFacts
{
    [Fact]
    public async Task Parse_Sdl()
    {
        /* Given */
        var sdl =
            @"
                    """"""
                    tanka_import from ""tanka://cost-analysis""
                    """"""

                    type ObjectType {
	                    property: Int! @cost(complexity: 1)
                    }

                    type Query {
                        obj: ObjectType
                    }
                 ";

        /* When */
        var schema = await new SchemaBuilder()
            .Add(sdl)
            // BuiltIn import providers are used
            .Build(new SchemaBuildOptions());


        /* Then */
        var objectType = schema.GetRequiredNamedType<ObjectDefinition>("ObjectType");
        var property = schema.GetField(objectType.Name, "property");
        Assert.Single(property.Directives, directive => directive.Name == "cost");
    }
}
using System.Threading.Tasks;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Extensions
{
    public class CostAnalysisImportFacts
    {
        [Fact]
        public async Task Parse_Sdl()
        {
            /* Given */
            var sdl = 
                @"# @import(path:""tanka://cost-analysis"")

                    type ObjectType {
	                    property: Int! @cost(complexity: 1)
                    }

                    type Query {
                        obj: ObjectType
                    }
                    ";

            /* When */
            var builder = await new SchemaBuilder()
                .SdlAsync(sdl);

            var schema = builder.Build();

            /* Then */
            var objectType = schema.GetNamedType<ObjectType>("ObjectType");
            var property = schema.GetField(objectType.Name, "property");
            Assert.Single(property.Directives, directive => directive.Name == "cost");
        }
    }
}
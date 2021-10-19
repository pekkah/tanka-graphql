using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Tests.Tools
{
    public class MakeExecutableWithIntrospectionFacts
    {
        [Fact]
        public async Task WithCustomScalar()
        {
            /* Given */
            var builder = new SchemaBuilder();
            await builder.SdlAsync(@"
                    scalar Date

                    input InputTest {
                        timestamp: Date
                    }

                    type Query {
                        getDate(date: Date): String 
                    }

                    type Mutation {
                        addDate(date: Date, inputTest: InputTest): String
                    }

                    schema {
                        query: Query
                        mutation: Mutation
                        subscription: Subscription
                    }");
               

            /* When */
            var schema = SchemaTools.MakeExecutableSchemaWithIntrospection(
                builder,
                converters: new Dictionary<string, IValueConverter>()
                {
                    ["Date"] = new StringConverter()
                });

            /* Then */
            var date = schema.GetNamedType("Date");

            Assert.NotNull(date);
            Assert.IsType<ScalarType>(date);
        }
    }
}
using System.Linq;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Tools
{
    public class MergeSchemasFacts
    {
        [Fact]
        public void should_include_fields_if_no_conflict()
        {
            /* Given */
            var left = new SchemaBuilder()
                .Query(out var leftQuery)
                .Connections(connect => connect
                    .Field(leftQuery, "left", ScalarType.Int));

            var right = new SchemaBuilder()
                .Query(out var rightQuery)
                .Connections(connect => connect
                    .Field(rightQuery, "right", ScalarType.String))
                .Build();


            /* When */
            var mergedSchema = left.Merge(right).Build();
            var queryFields = mergedSchema.GetFields(mergedSchema.Query.Name)
                .ToList();

            /* Then */
            Assert.Single(queryFields, pair => pair.Key == "left");
            Assert.Single(queryFields, pair => pair.Key == "right");
        }

        [Fact]
        public void Merge_schemas()
        {
            /* Given */
            var schemaOne = new SchemaBuilder()
                .Sdl(@"
                    input RightInput {
                        rightField: String!
                    }

                    type RightTwo {
                        rightField(input: RightInput): Int!
                    }

                    type Query {
                        rightField: RightTwo
                    }

                    schema {
                        query: Query
                    }
                    ")
                .Build();

            var builder = new SchemaBuilder()
                .Sdl(@"
                    type Query {
                        leftField: Int!
                    }

                    schema {
                        query: Query
                    }
                    ");

            
            /* When */
            var schema = builder.Merge(schemaOne)
                .Build();

            /* Then */
            var rightInput = schema.GetNamedType<InputObjectType>("RightInput");
            Assert.NotNull(rightInput);
        }
    }
}
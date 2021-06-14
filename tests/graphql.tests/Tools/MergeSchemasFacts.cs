using System.Linq;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Tests.Tools
{
    public class MergeSchemasFacts
    {
        [Fact]
        public void Should_include_fields_if_no_conflict()
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

        [Fact]
        public void Merge_schema_with_new_enum()
        {
            /* Given */
            var newTypes = new SchemaBuilder()
                .Sdl(@"
                    enum COLOR {
                        RED
                        BLUE
                        GREEN
                        }

                    type Query {
                        currentColor: COLOR
                    }     
                    ")
                .Build();

            var builder = new SchemaBuilder()
                .Sdl(@"
                    type Query {
                        name: String!
                    }

                    schema {
                        query: Query
                    }
                    ");

            
            /* When */
            var schema = builder.Merge(newTypes)
                .Build();

            /* Then */
            var newEnumType = schema.GetNamedType<EnumType>("COLOR");
            Assert.NotNull(newEnumType);

            var field = schema.GetField("Query", "currentColor");
            Assert.Equal(newEnumType, field.Type);
        }

        [Fact]
        public void Merge_schema_with_new_union()
        {
            /* Given */
            var newTypes = new SchemaBuilder()
                .Sdl(@"
type Red {
}

type Green {
}

type Orange {
}

union Color = Red | Orange | Green

type Query {
    color: Color
}
                    ")
                .Build();

            var builder = new SchemaBuilder()
                .Sdl(@"
                    type Query {
                    }
                    ");

            
            /* When */
            var schema = builder.Merge(newTypes)
                .Build();

            /* Then */
            var newUnionType = schema.GetNamedType<UnionType>("Color");
            Assert.NotNull(newUnionType);

            var field = schema.GetField("Query", "color");
            Assert.Equal(newUnionType, field.Type);
        }

        [Fact]
        public void Merge_schema_with_new_CustomScalar()
        {
            /* Given */
            var newTypes = new SchemaBuilder()
                .Sdl(@"
                    scalar Date

                    input InputTest {
                        timestamp: Date
                    }   

                    type Query {
                        useRaw(date: Date!): Int
                        useWithInput(inputWithDate: InputTest!): Int
                    }
                    ")
                .Include("Date", new StringConverter())
                .Build();

            var builder = new SchemaBuilder()
                .Sdl(@"
                    schema {
                        query: Query
                    }
                    ");


            /* When */
            var schema = builder.Merge(newTypes)
                .Build();

            /* Then */
            var newScalarType = schema.GetNamedType<ScalarType>("Date");
            Assert.NotNull(newScalarType);
        }
    }
}
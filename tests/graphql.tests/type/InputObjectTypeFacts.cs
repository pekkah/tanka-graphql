using System.Collections.Generic;
using tanka.graphql.execution;
using tanka.graphql.schema;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class InputObjectTypeFacts
    {
        public InputObjectTypeFacts()
        {
            _builder = new SchemaBuilder();
            _builder.Query(out _);
        }

        private readonly SchemaBuilder _builder;

        [Fact]
        public void Define()
        {
            /* Given */
            /* When */
            _builder.InputObject("ExampleInputObject", out var input)
                .Connections(connect => connect
                    .InputField(input, "a", ScalarType.Boolean));

            var schema = _builder.Build();

            /* Then */
            var inputFields = schema.GetInputFields(input.Name);
            Assert.Single(inputFields,
                fk => fk.Key == "a"
                      && (ScalarType) fk.Value.Type == ScalarType.Boolean);
        }

        [Fact]
        public void Input_coercion()
        {
            /* Given */
            _builder.InputObject("ExampleInputObject", out var input)
                .Connections(connect => connect
                    .InputField(input, "a", ScalarType.String)
                    .InputField(input, "b", ScalarType.Int));

            var schema = _builder.Build();

            /* When */
            var literalValue = new Dictionary<string, object>
            {
                ["a"] = "abc",
                ["b"] = 123
            };

            var actual =
                (Dictionary<string, object>) Values.CoerceValue(
                    schema.GetInputFields, 
                    literalValue, 
                    input);

            /* Then */
            foreach (var expectedKv in literalValue)
            {
                Assert.True(actual.ContainsKey(expectedKv.Key));
                Assert.Equal(expectedKv.Value, actual[expectedKv.Key]);
            }
        }
    }
}
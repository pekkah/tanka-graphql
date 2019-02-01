using System.Collections.Generic;
using tanka.graphql.execution;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class InputObjectTypeFacts
    {
        [Fact]
        public void Define()
        {
            /* Given */
            /* When */
            var input = new InputObjectType(
                "ExampleInputObject",
                new InputFields
                {
                    {"a", ScalarType.Boolean}
                });

            /* Then */
            Assert.Single(input.Fields, fk => fk.Key == "a"
                                              && (ScalarType) fk.Value.Type == ScalarType.Boolean);
        }

        [Fact]
        public void Input_coercion()
        {
            /* Given */
            var input = new InputObjectType(
                "ExampleInputObject",
                new InputFields
                {
                    {"a", ScalarType.String},
                    {"b", ScalarType.NonNullInt}
                });

            /* When */
            var literalValue = new Dictionary<string, object>
            {
                ["a"] = "abc",
                ["b"] = 123L
            };

            Dictionary<string, object> actual = 
                (Dictionary<string, object>)Values.CoerceValue(literalValue, input);

            /* Then */
            foreach (var expectedKv in literalValue)
            {
                Assert.True(actual.ContainsKey(expectedKv.Key));
                Assert.Equal(expectedKv.Value, actual[expectedKv.Key]);
            }
        }
    }
}
using System.Collections.Generic;
using System.Text.Json;
using Tanka.GraphQL.DTOs;
using Xunit;

namespace Tanka.GraphQL.Tests.DTOs
{
    public class ObjectDictionaryConverterFacts
    {
        private readonly JsonSerializerOptions _options;

        public ObjectDictionaryConverterFacts()
        {
            _options = new JsonSerializerOptions()
            {
                Converters =
                {
                    new ObjectDictionaryConverter()
                }
            };
        }

        [Fact]
        public void Deserialize_SimpleValues()
        {
            /* Given */
            var json = @"
                {
                    ""int"": 123,
                    ""double"": 123.456,
                    ""string"": ""string"",
                    ""bool"": true
                }
            ";

            /* When */
            var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

            /* Then */
            Assert.Equal(123, actual["int"]);
            Assert.Equal(123.456, actual["double"]);
            Assert.Equal("string", actual["string"]);
            Assert.Equal(true, actual["bool"]);
        }

        [Fact]
        public void Deserialize_ComplexValue()
        {
            /* Given */
            var json = @"
                {
                    ""complex"": {
                        ""int"": 123,
                        ""double"": 123.456,
                        ""string"": ""string"",
                        ""bool"": true
                    }
                }
            ";

            /* When */
            var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

            /* Then */
            Assert.IsAssignableFrom<IDictionary<string, object>>(actual["complex"]);
            var complex = (IDictionary<string, object>)actual["complex"];
            Assert.Equal(123, complex["int"]);
            Assert.Equal(123.456, complex["double"]);
            Assert.Equal("string", complex["string"]);
            Assert.Equal(true, complex["bool"]);
        }

        [Fact]
        public void Deserialize_MixedValue()
        {
            /* Given */
            var json = @"
                {
                    ""int"": 123,
                    ""complex"": {
                        ""int"": 123,
                        ""double"": 123.456,
                        ""string"": ""string"",
                        ""bool"": true
                    },
                    ""bool"": true
                }
            ";

            /* When */
            var actual = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);

            /* Then */
            Assert.Equal(123, actual["int"]);
            Assert.Equal(true, actual["bool"]);

            Assert.IsAssignableFrom<IDictionary<string, object>>(actual["complex"]);
            var complex = (IDictionary<string, object>)actual["complex"];
            Assert.Equal(123, complex["int"]);
            Assert.Equal(123.456, complex["double"]);
            Assert.Equal("string", complex["string"]);
            Assert.Equal(true, complex["bool"]);
        }
    }
}
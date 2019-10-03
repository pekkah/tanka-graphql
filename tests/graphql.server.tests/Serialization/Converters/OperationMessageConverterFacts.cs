using System.Collections.Generic;
using System.Text.Json;
using Tanka.GraphQL.DTOs;
using Tanka.GraphQL.Server.Serialization.Converters;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Xunit;

namespace Tanka.GraphQL.Server.Tests.Serialization.Converters
{
    public class OperationMessageConverterFacts
    {
        public OperationMessageConverterFacts()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new OperationMessageConverter(),
                    new ObjectDictionaryConverter()
                }
            };
        }

        private readonly JsonSerializerOptions _options;

        private string CreateMessageJson(string id, string type, string payloadJson = null)
        {
            return @"
                {
                    ""id"": ""{id}"",
                    ""type"": ""{type}"",
                    ""payload"": {payloadJson}
                }
                "
                .Replace("{id}", id)
                .Replace("{type}", type)
                .Replace("{payloadJson}", payloadJson ?? "null");
        }

        [Fact]
        public void Deserialize_Init()
        {
            /* Given */
            var json = CreateMessageJson(
                "1",
                MessageType.GQL_CONNECTION_INIT,
                @"
                {
                    ""token"": ""123""
                }
                ");

            /* When */
            var actual = JsonSerializer.Deserialize<OperationMessage>(json, _options);

            /* Then */
            Assert.Equal("1", actual.Id);
            Assert.Equal(MessageType.GQL_CONNECTION_INIT, actual.Type);
            Assert.IsType<Dictionary<string, object>>(actual.Payload);
        }

        [Fact]
        public void Deserialize_Start()
        {
            /* Given */
            var json = CreateMessageJson(
                "1",
                MessageType.GQL_START,
                @"
                {
                    ""token"": ""123""
                }
                ");

            /* When */
            var actual = JsonSerializer.Deserialize<OperationMessage>(json, _options);

            /* Then */
            Assert.Equal("1", actual.Id);
            Assert.Equal(MessageType.GQL_START, actual.Type);
            Assert.IsType<OperationMessageQueryPayload>(actual.Payload);
        }

        [Fact]
        public void Deserialize_Stop()
        {
            /* Given */
            var json = CreateMessageJson(
                "1",
                MessageType.GQL_STOP);

            /* When */
            var actual = JsonSerializer.Deserialize<OperationMessage>(json, _options);

            /* Then */
            Assert.Equal("1", actual.Id);
            Assert.Equal(MessageType.GQL_STOP, actual.Type);
            Assert.Null(actual.Payload);
        }
    }
}
using System.Collections.Generic;
using System.Text.Json;
using Tanka.GraphQL.Server.Links.DTOs;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Tanka.GraphQL.Server.WebSockets.DTOs.Serialization.Converters;
using Xunit;

namespace Tanka.GraphQL.Server.Tests.WebSockets.DTOs.Serialization.Converters;

public class OperationMessageConverterFacts
{
    private readonly JsonSerializerOptions _options;

    public OperationMessageConverterFacts()
    {
        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new OperationMessageConverter(),
                new ObjectDictionaryConverter()
            }
        };
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
    public void Deserialize_Data()
    {
        /* Given */
        var json = CreateMessageJson(
            "1",
            MessageType.GQL_DATA,
            @"
                {
                    ""data"": 
                    {
                        ""field"": 123    
                    }
                }
                ");

        /* When */
        var actual = JsonSerializer.Deserialize<OperationMessage>(json, _options);

        /* Then */
        Assert.Equal("1", actual.Id);
        Assert.Equal(MessageType.GQL_DATA, actual.Type);
        Assert.IsType<ExecutionResult>(actual.Payload);
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

    [Fact]
    public void Serialize_Data()
    {
        /* Given */
        var message = new OperationMessage
        {
            Id = "1",
            Type = MessageType.GQL_DATA,
            Payload = new ExecutionResult
            {
                Data = new Dictionary<string, object>
                {
                    ["field"] = "123"
                }
            }
        };

        /* When */
        var actual = JsonSerializer.Serialize(message, _options);


        /* Then */
        message.ShouldMatchJson(actual);
    }

    [Fact]
    public void Serialize_Complete()
    {
        /* Given */
        var message = new OperationMessage
        {
            Id = "1",
            Type = MessageType.GQL_COMPLETE
        };

        /* When */
        var actual = JsonSerializer.Serialize(message, _options);


        /* Then */
        message.ShouldMatchJson(actual);
    }

    [Fact]
    public void Serialize_Error()
    {
        /* Given */
        var message = new OperationMessage
        {
            Id = "1",
            Type = MessageType.GQL_CONNECTION_ERROR,
            Payload = new ExecutionResult
            {
                Errors = new List<ExecutionError>
                {
                    new()
                    {
                        Message = "error"
                    }
                }
            }
        };

        /* When */
        var actual = JsonSerializer.Serialize(message, _options);


        /* Then */
        message.ShouldMatchJson(actual);
    }

    private string CreateMessageJson(string id, string type, string payloadJson = null)
    {
        return 
            @"{
    ""id"": ""{id}"",
    ""payload"": {payloadJson},
    ""type"": ""{type}""
 }"
            .Replace("{id}", id)
            .Replace("{type}", type)
            .Replace("{payloadJson}", payloadJson ?? "null");
    }
}
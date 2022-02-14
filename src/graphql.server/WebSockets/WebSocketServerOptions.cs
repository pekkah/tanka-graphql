using System;
using System.Text.Json;
using System.Threading.Tasks;
using Tanka.GraphQL.Server.Links.DTOs;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Tanka.GraphQL.Server.WebSockets.DTOs.Serialization.Converters;

namespace Tanka.GraphQL.Server.WebSockets;

public class WebSocketServerOptions
{
    /// <summary>
    ///     Method called when `connection_init` message is received from client to validate
    ///     the connectionParams
    /// </summary>
    /// <returns>true if connection accepted; otherwise false</returns>
    public Func<MessageContext, Task> AcceptAsync { get; set; } = async context =>
    {
        await context.Output.WriteAsync(new OperationMessage
        {
            Type = MessageType.GQL_CONNECTION_ACK
        });
    };

    public JsonSerializerOptions MessageSerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new ObjectDictionaryConverter(),
            new OperationMessageConverter()
        }
    };
}
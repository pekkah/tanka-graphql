using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using Tanka.GraphQL.Server.Links.DTOs;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Tanka.GraphQL.Server.WebSockets.DTOs.Serialization.Converters;

namespace Tanka.GraphQL.Server.WebSockets
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
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

        public JsonSerializerOptions MessageSerializerOptions { get; set; } = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new ObjectDictionaryConverter(),
                new OperationMessageConverter()
            }
        };
    }
}
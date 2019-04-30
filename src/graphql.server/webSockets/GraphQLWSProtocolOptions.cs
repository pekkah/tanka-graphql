using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GraphQLWSProtocolOptions
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
    }
}
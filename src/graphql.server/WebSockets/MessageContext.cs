using System.Threading.Channels;
using Tanka.GraphQL.Server.WebSockets.DTOs;

namespace Tanka.GraphQL.Server.WebSockets;

public class MessageContext
{
    public MessageContext(OperationMessage message, ChannelWriter<OperationMessage> output)
    {
        Message = message;
        Output = output;
    }

    public OperationMessage Message { get; }

    public ChannelWriter<OperationMessage> Output { get; }
}
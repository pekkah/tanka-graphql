using System.Threading.Channels;
using System.Threading.Tasks;
using Tanka.GraphQL.Server.WebSockets;
using Tanka.GraphQL.Server.WebSockets.DTOs;

namespace Tanka.GraphQL.Server.Tests.WebSockets;

public class MessageSinkProtocol : IProtocolHandler
{
    private readonly Channel<OperationMessage> _messages = Channel.CreateUnbounded<OperationMessage>();

    public ChannelReader<OperationMessage> Input => _messages.Reader;

    public ValueTask Handle(MessageContext context)
    {
        return _messages.Writer.WriteAsync(context.Message);
    }
}
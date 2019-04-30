using System.Threading.Channels;
using System.Threading.Tasks;
using tanka.graphql.server.webSockets;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.tests.webSockets
{
    public class MessageSinkProtocol : IProtocolHandler
    {
        private readonly Channel<OperationMessage> _messages = Channel.CreateUnbounded<OperationMessage>();

        public ChannelReader<OperationMessage> Input => _messages.Reader;

        public ValueTask Handle(MessageContext context)
        {
            return _messages.Writer.WriteAsync(context.Message);
        }
    }
}
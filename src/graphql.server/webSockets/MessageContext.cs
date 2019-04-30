using System.Threading.Channels;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
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
}
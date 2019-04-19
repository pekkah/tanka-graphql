using System.Threading.Channels;
using System.Threading.Tasks;
using tanka.graphql.server.webSockets;

namespace graphql.server.tests.host
{
    public class MessageServer : ITextProtocolApplication
    {
        public Channel<string> Messages { get; } = Channel.CreateUnbounded<string>();

        public ValueTask OnMessage(string message)
        {
            return Messages.Writer.WriteAsync(message);
        }
    }
}
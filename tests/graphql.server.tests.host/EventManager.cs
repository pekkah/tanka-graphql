using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace graphql.server.tests.host
{
    public class EventManager
    {
        private List<Channel<object>> _channels = new List<Channel<object>>();

        private List<string> _messageBuffer = new List<string>();

        public async Task Hello(string message)
        {
            _messageBuffer.Add(message);
            foreach (var channel in _channels)
            {
                await channel.Writer.WriteAsync(message);
            }
        }

        public async Task<ChannelReader<object>> Subscribe(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<object>();
            _channels.Add(channel);
            cancellationToken.Register(() =>
            {
                _channels.Remove(channel);
                channel.Writer.Complete();
            });

            foreach (var previousMessage in _messageBuffer)
            {
                await channel.Writer.WriteAsync(previousMessage);
            }

            return channel.Reader;
        }
    }
}
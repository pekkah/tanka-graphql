using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace graphql.server.tests.host
{
    public class EventManager
    {
        private readonly ConcurrentDictionary<string, Channel<object>> _channels = new ConcurrentDictionary<string, Channel<object>>();

        private readonly List<string> _messageBuffer = new List<string>();

        public async Task Hello(string message)
        {
            _messageBuffer.Add(message);
            foreach (var kv in _channels)
            {
                var channel = kv.Value;
                if(!channel.Reader.Completion.IsCompleted)
                    await channel.Writer.WriteAsync(message);
            }
        }

        public void Clear()
        {
            _messageBuffer.Clear();
        }

        public async Task<ChannelReader<object>> Subscribe(string id, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<object>();
            _channels[id]= channel;
            cancellationToken.Register(() =>
            {
                if (_channels.TryRemove(id, out var remove))
                    remove.Writer.TryComplete();
            });

            foreach (var previousMessage in _messageBuffer)
            {
                await channel.Writer.WriteAsync(previousMessage);
            }

            return channel.Reader;
        }
    }
}
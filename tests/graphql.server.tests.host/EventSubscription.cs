using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace graphql.server.tests.host
{
    public class EventSubscription
    {
        private readonly Channel<object> _channel;

        public EventSubscription(int id)
        {
            Id = id;
            _channel = Channel.CreateUnbounded<object>();
        }

        public void Unsubscribe()
        {
            _channel.Writer.TryComplete();
        }

        public void Error(Exception error)
        {
            _channel.Writer.TryComplete(error);
        }

        public int Id { get; }

        public ChannelReader<object> Reader => _channel.Reader;

        public ValueTask HelloAsync(string message)
        {
            return _channel.Writer.WriteAsync(message);
        }
    }
}
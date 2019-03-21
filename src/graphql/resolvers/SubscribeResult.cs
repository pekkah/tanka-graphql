using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace tanka.graphql.resolvers
{
    public class SubscribeResult : ISubscribeResult
    {
        private readonly Channel<object> _channel;

        public SubscribeResult()
        {
            _channel = Channel.CreateUnbounded<object>();
        }

        public ChannelReader<object> GetReader()
        {
            return _channel.Reader;
        }

        public bool TryComplete(Exception error = null)
        {
            return _channel.Writer.TryComplete(error);
        } 


        public ValueTask WriteAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            return _channel.Writer.WriteAsync(item, cancellationToken);
        }
    }
}
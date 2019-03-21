using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace tanka.graphql.resolvers
{
    public interface ISubscribeResult
    {
        ValueTask WriteAsync<T>(T item, CancellationToken cancellationToken);

        ChannelReader<object> GetReader();

        bool TryComplete(Exception error = null);
    }
}
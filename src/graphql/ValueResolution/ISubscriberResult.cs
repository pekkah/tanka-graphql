using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public interface ISubscriberResult
    {
        ValueTask WriteAsync<T>(T item, CancellationToken cancellationToken);

        ChannelReader<object> Reader { get; }

        bool TryComplete(Exception error = null);
    }
}
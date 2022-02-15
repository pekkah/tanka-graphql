using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution;

public interface ISubscriberResult
{
    ChannelReader<object> Reader { get; }
    ValueTask WriteAsync<T>(T item, CancellationToken cancellationToken);

    bool TryComplete(Exception error = null);
}
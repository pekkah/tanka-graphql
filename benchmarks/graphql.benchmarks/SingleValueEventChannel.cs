using System.Threading;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Benchmarks
{
    public class SingleValueEventChannel : EventChannel<string>
    {
        public override void OnSubscribed(ISubscriberResult subscription)
        {
            subscription.WriteAsync("value", CancellationToken.None)
                .AsTask()
                .Wait();
        }
    }
}
using System.Threading;
using tanka.graphql.channels;
using tanka.graphql.resolvers;

namespace tanka.graphql.benchmarks
{
    public class SingleValueEventChannel : EventChannel<string>
    {
        public override void OnSubscribed(ISubscribeResult subscription)
        {
            subscription.WriteAsync("value", CancellationToken.None)
                .AsTask()
                .Wait();
        }
    }
}
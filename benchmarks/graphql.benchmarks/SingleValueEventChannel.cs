using tanka.graphql.channels;
using tanka.graphql.resolvers;

namespace tanka.graphql.benchmarks
{
    public class SingleValueEventChannel : EventChannel<string>
    {
        public override void OnSubscribed(SubscribeResult subscription)
        {
            subscription.WriteAsync("value").AsTask().Wait();
        }
    }
}
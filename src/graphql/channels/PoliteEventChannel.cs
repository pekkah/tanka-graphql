using System.Threading;
using tanka.graphql.resolvers;

namespace tanka.graphql.channels
{
    public class PoliteEventChannel<T> : EventChannel<T>
    {
        private readonly T _byeItem;
        private readonly T _welcomeItem;

        public PoliteEventChannel(T welcomeItem, T byeItem = default)
        {
            _welcomeItem = welcomeItem;
            _byeItem = byeItem;
        }
        
        public override void OnSubscribed(ISubscribeResult subscription)
        {
            if (!_welcomeItem.Equals(default(T)))
                subscription.WriteAsync(_welcomeItem, CancellationToken.None);
        }

        public override void OnUnsubscribing(ISubscribeResult subscription)
        {
            if (!_byeItem.Equals(default(T)))
                subscription.WriteAsync(_byeItem, CancellationToken.None);
        }
    }
}
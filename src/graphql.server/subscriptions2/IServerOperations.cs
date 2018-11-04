using System.Threading.Tasks;

namespace fugu.graphql.server.subscriptions
{
    public interface IServerOperations
    {
        IMessageTransport Transport { get; }

        ISubscriptionManager Subscriptions { get; }
    }
}
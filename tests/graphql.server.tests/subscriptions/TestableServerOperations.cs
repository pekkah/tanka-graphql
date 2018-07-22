using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions;

namespace fugu.graphql.server.tests.subscriptions
{
    public class TestableServerOperations : IServerOperations
    {
        public TestableServerOperations(
            IMessageTransport transport,
            ISubscriptionManager subscriptions)
        {
            Subscriptions = subscriptions;
            Transport = transport;
        }

        public Task Terminate()
        {
            IsTerminated = true;
            return Task.CompletedTask;
        }

        public bool IsTerminated { get; set; }

        public IMessageTransport Transport { get; }

        public ISubscriptionManager Subscriptions { get; }
    }
}
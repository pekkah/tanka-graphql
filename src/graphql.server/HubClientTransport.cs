using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions;

namespace fugu.graphql.server
{
    public class HubClientTransport : IMessageTransport
    {
        private readonly BufferBlock<OperationMessage> _receivedMessages;

        public HubClientTransport(IServerClient client)
        {
            Writer = new ActionBlock<OperationMessage>(async message => { await client.Data(message); });
            _receivedMessages = new BufferBlock<OperationMessage>();
        }

        public ISourceBlock<OperationMessage> Reader => _receivedMessages;

        public ITargetBlock<OperationMessage> Writer { get; }

        public void Complete()
        {
            Writer.Complete();
        }

        public Task Completion => Writer.Completion;

        public Task ConsumeMessage(OperationMessage operation)
        {
            return _receivedMessages.SendAsync(operation);
        }
    }
}
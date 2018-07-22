using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions;

namespace fugu.graphql.server.tests.subscriptions
{
    public class TestableSubscriptionTransport : IMessageTransport
    {
        private readonly BufferBlock<OperationMessage> _readBuffer;

        public TestableSubscriptionTransport()
        {
            Writer = new ActionBlock<OperationMessage>(message => WrittenMessages.Add(message), new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 1,
                EnsureOrdered = true
            });
            Reader = _readBuffer = new BufferBlock<OperationMessage>(new DataflowBlockOptions()
            {
                EnsureOrdered = true
            });
        }

        public List<OperationMessage> WrittenMessages { get; } = new List<OperationMessage>();

        public ISourceBlock<OperationMessage> Reader { get; }

        public ITargetBlock<OperationMessage> Writer { get; }

        public void Complete()
        {
            Reader.Complete();
        }

        public Task Completion => Task.WhenAll(Reader.Completion, Writer.Completion);

        public bool AddMessageToRead(OperationMessage message)
        {
            return _readBuffer.Post(message);
        }
    }
}
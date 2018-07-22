using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace fugu.graphql.resolvers
{
    public class SubscribeResult : ISubscribeResult
    {
        private readonly Func<Task> _unsubscribeAsync;

        public ISourceBlock<object> Reader { get; set; }

        public Task UnsubscribeAsync()
        {
            return _unsubscribeAsync();
        }

        public SubscribeResult(ISourceBlock<object> reader, Func<Task> unsubscribeAsync)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _unsubscribeAsync = unsubscribeAsync ?? throw new ArgumentNullException(nameof(unsubscribeAsync));
        }
    }
}
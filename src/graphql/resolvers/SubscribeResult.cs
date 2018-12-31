using System;
using System.Threading.Tasks.Dataflow;

namespace tanka.graphql.resolvers
{
    public class SubscribeResult : ISubscribeResult
    {
        public SubscribeResult(ISourceBlock<object> reader)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public ISourceBlock<object> Reader { get; set; }
    }
}
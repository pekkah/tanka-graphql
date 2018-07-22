using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace fugu.graphql.resolvers
{
    public interface ISubscribeResult
    {
        ISourceBlock<object> Reader { get; }

        Task UnsubscribeAsync();
    }
}
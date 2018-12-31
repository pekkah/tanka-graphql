using System.Threading.Tasks.Dataflow;

namespace tanka.graphql.resolvers
{
    public interface ISubscribeResult
    {
        ISourceBlock<object> Reader { get; }
    }
}
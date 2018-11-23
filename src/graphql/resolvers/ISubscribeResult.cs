using System.Threading.Channels;

namespace fugu.graphql.resolvers
{
    public interface ISubscribeResult
    {
        ChannelReader<object> Reader { get; }
    }
}
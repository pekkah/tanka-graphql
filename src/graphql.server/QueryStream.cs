using System.Threading.Channels;

namespace Tanka.GraphQL.Server
{
    public class QueryStream
    {
        public QueryStream(ChannelReader<ExecutionResult> reader)
        {
            Reader = reader;
        }

        public ChannelReader<ExecutionResult> Reader { get; }
    }
}
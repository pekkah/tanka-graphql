using System.Threading.Channels;

namespace tanka.graphql.server
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
using System.Threading.Channels;

namespace fugu.graphql.server
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
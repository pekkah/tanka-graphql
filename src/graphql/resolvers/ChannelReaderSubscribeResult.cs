using System;
using System.Threading.Channels;

namespace fugu.graphql.resolvers
{
    public class ChannelReaderSubscribeResult : ISubscribeResult
    {
        public ChannelReaderSubscribeResult(ChannelReader<object> reader)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public ChannelReader<object> Reader { get; set; }
    }
}
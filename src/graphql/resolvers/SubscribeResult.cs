using System;
using System.Threading.Channels;

namespace fugu.graphql.resolvers
{
    public class SubscribeResult : ISubscribeResult
    {
        public SubscribeResult(ChannelReader<object> reader)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public ChannelReader<object> Reader { get; set; }
    }
}
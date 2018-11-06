﻿using System.Threading.Channels;

namespace fugu.graphql.server
{
    public class QueryStream
    {
        public QueryStream(Channel<ExecutionResult> channel)
        {
            Channel = channel;
        }

        public Channel<ExecutionResult> Channel { get; }
    }
}
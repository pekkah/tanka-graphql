using System;
using System.Collections.Generic;

namespace fugu.graphql.tracing
{
    public class TraceExtensionRecord
    {
        public int Version => 1;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public long Duration { get; set; }

        public OperationTrace Parsing { get; set; }

        public OperationTrace Validation { get; set; }

        public ExecutionTrace Execution { get; set; } = new ExecutionTrace();

        public class OperationTrace
        {
            public long StartOffset { get; set; }

            public long Duration { get; set; }
        }

        public class ExecutionTrace
        {
            public List<ResolverTrace> Resolvers { get; set; } = new List<ResolverTrace>();
        }

        public class ResolverTrace : OperationTrace
        {
            public List<object> Path { get; set; } = new List<object>();

            public string ParentType { get; set; }

            public string FieldName { get; set; }

            public string ReturnType { get; set; }
        }
    }
}
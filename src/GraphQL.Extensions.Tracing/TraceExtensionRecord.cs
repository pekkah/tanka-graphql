using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.Extensions.Tracing;

public class TraceExtensionRecord
{
    public long Duration { get; set; }

    public DateTime EndTime { get; set; }

    public ExecutionTrace Execution { get; set; } = new();

    public OperationTrace Parsing { get; set; }

    public DateTime StartTime { get; set; }

    public OperationTrace Validation { get; set; }
    public int Version => 1;

    public class ExecutionTrace
    {
        public List<ResolverTrace> Resolvers { get; set; } = new();
    }

    public class OperationTrace
    {
        public long Duration { get; set; }
        public long StartOffset { get; set; }
    }

    public class ResolverTrace : OperationTrace
    {
        public string FieldName { get; set; }

        public string ParentType { get; set; }
        public List<object> Path { get; set; } = new();

        public string ReturnType { get; set; }
    }
}
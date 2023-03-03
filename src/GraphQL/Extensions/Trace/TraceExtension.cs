using System.Diagnostics;

namespace Tanka.GraphQL.Extensions.Trace
{
    public class TraceExtension
    {
        public DateTime UtcStart { get; set; }

        public double Elapsed { get; set; }
    }

    public static class TraceOperationDelegateBuilderExtensions
    {
        public static OperationDelegateBuilder UseTrace(this OperationDelegateBuilder builder)
        {
            return builder.Use(next => async context =>
            {
                var trace = TraceFeature.StartNew();
                context.Features.Set<ITraceFeature>(trace);
                await next(context);
                var extension = trace.End();

                context.Response = AddTrace(context.Response, extension);

                static async IAsyncEnumerable<ExecutionResult> AddTrace(
                    IAsyncEnumerable<ExecutionResult> source,
                    TraceExtension extension)
                {
                    await using var e = source.GetAsyncEnumerator();

                    // we only append the trace to the first result
                    if (await e.MoveNextAsync())
                    {
                        var current = e.Current;
                        var extensions = current.Extensions != null
                            ? new Dictionary<string,object>(current.Extensions)
                            : new Dictionary<string, object>(1);

                        extensions["trace"] = extension;
                        yield return current with
                        {
                            Extensions = extensions
                        };
                    }

                    // the rest
                    while (await e.MoveNextAsync())
                        yield return e.Current;
                }
            });
        }
    }

    public interface ITraceFeature
    {

    }

    public class TraceFeature : ITraceFeature
    {
        public static TraceFeature StartNew()
        {
            return new TraceFeature()
            {
                Stopwatch = Stopwatch.StartNew(),
                UtcStart = DateTime.UtcNow
            };
        }

        public DateTime UtcStart { get; set; }

        public Stopwatch Stopwatch { get; set; }

        public TraceExtension End()
        {
            Stopwatch.Stop();
            return new TraceExtension()
            {
                UtcStart = UtcStart,
                Elapsed = Stopwatch.Elapsed.TotalSeconds
            };
        }
    }
}
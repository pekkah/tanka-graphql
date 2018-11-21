using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.validation;
using GraphQLParser.AST;

namespace fugu.graphql.tracing
{
    public class TraceExtension : ExtensionBase
    {
        private DateTime _startTime;
        private Stopwatch _stopwatch;
        private TimeSpan _validationStarted;
        private TimeSpan _validationEnded;
        private TimeSpan _parsingStarted;
        private TimeSpan _parsingEnded;
        private readonly Func<DateTime> _utcNow;
        private readonly List<TraceExtensionRecord.ResolverTrace> _resolverTraces = new List<TraceExtensionRecord.ResolverTrace>();

        public TraceExtension()
        {
            _utcNow = () => DateTime.UtcNow;
        }

        public TraceExtension(Func<DateTime> utcNow)
        {
            _utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
        }

        public override Task BeginExecuteAsync(ExecutionOptions options)
        {
            _startTime = _utcNow();
            _stopwatch = Stopwatch.StartNew();
            return Task.CompletedTask;
        }

        public override Task BeginValidationAsync()
        {
            _validationStarted = _stopwatch.Elapsed;
            return Task.CompletedTask;
        }

        public override Task EndValidationAsync(ValidationResult validationResult)
        {
            _validationEnded = _stopwatch.Elapsed;
            return Task.CompletedTask;
        }

        public override Task EndExecuteAsync(ExecutionResult executionResult)
        {
            _stopwatch.Stop();

            // execution
            var endTime = _startTime.Add(_stopwatch.Elapsed);
            var duration = endTime - _startTime;

            // parsing
            var parsingDuration = _parsingEnded - _parsingStarted;

            //validation
            var validationDuration = _validationEnded - _validationStarted;

            var record = new TraceExtensionRecord()
            {
                Duration = duration.TotalNanoSeconds(),
                StartTime = _startTime,
                EndTime = endTime,
                Parsing = new TraceExtensionRecord.OperationTrace()
                {
                    StartOffset = _parsingStarted.TotalNanoSeconds(),
                    Duration = parsingDuration.TotalNanoSeconds()
                },
                Validation = new TraceExtensionRecord.OperationTrace()
                {
                    StartOffset = _validationStarted.TotalNanoSeconds(),
                    Duration = validationDuration.TotalNanoSeconds()
                },
                Execution = new TraceExtensionRecord.ExecutionTrace()
                {
                    Resolvers = _resolverTraces
                }
            };

            executionResult.AddExtension("tracing", record);
            return Task.CompletedTask;
        }

        public override Task BeginParseDocumentAsync()
        {
            _parsingStarted = _stopwatch.Elapsed;
            return Task.CompletedTask;
        }

        public override Task EndParseDocumentAsync(GraphQLDocument document)
        {
            _parsingEnded = _stopwatch.Elapsed;
            return Task.CompletedTask;
        }

        public override Resolver Resolver(Resolver next)
        {
            return async context =>
            {
                var start = _stopwatch.Elapsed;
                var result = await next(context);
                var end = _stopwatch.Elapsed;

                _resolverTraces.Add(new TraceExtensionRecord.ResolverTrace()
                {
                    StartOffset = start.TotalNanoSeconds(),
                    Duration = (end -start).TotalNanoSeconds(),
                    ParentType = context.ObjectType.Name,
                    FieldName = context.FieldName,
                    Path = context.Path.Segments.ToList(),
                    ReturnType = context.Field.Type.Name
                });

                return result;
            };
        }
    }

    internal static class TimeSpanExtensions
    {
        public static long TotalNanoSeconds(this TimeSpan timeSpan)
        {
            return (long)(timeSpan.TotalMilliseconds * 1000 * 1000);
        }
    }
}
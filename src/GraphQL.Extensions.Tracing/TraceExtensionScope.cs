using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Extensions.Tracing
{
    public class TraceExtensionScope : IExtensionScope
    {
        private readonly DateTime _startTime;
        private readonly Stopwatch _stopwatch;
        private TimeSpan _validationStarted;
        private TimeSpan _validationEnded;
        private TimeSpan _parsingStarted;
        private TimeSpan _parsingEnded;
        private readonly List<TraceExtensionRecord.ResolverTrace> _resolverTraces = new List<TraceExtensionRecord.ResolverTrace>();

        public TraceExtensionScope(ExecutionOptions options):
            this(()=> DateTime.UtcNow, options)
        {
        }

        public TraceExtensionScope(Func<DateTime> utcNow, ExecutionOptions options)
        {
            if (utcNow == null) throw new ArgumentNullException(nameof(utcNow));

            _startTime = utcNow();
            _stopwatch = Stopwatch.StartNew();
        }

        public ValueTask BeginValidationAsync()
        {
            _validationStarted = _stopwatch.Elapsed;
            return default;
        }

        public ValueTask EndValidationAsync(ValidationResult validationResult)
        {
            _validationEnded = _stopwatch.Elapsed;
            return default;
        }

        public ValueTask EndExecuteAsync(IExecutionResult executionResult)
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
            return default;
        }

        public ValueTask BeginParseDocumentAsync()
        {
            _parsingStarted = _stopwatch.Elapsed;
            return default;
        }

        public ValueTask EndParseDocumentAsync(ExecutableDocument document)
        {
            _parsingEnded = _stopwatch.Elapsed;
            return default;
        }

        public ValueTask BeginResolveAsync(IResolverContext context)
        {
            var start = _stopwatch.Elapsed;
            var record = new TraceExtensionRecord.ResolverTrace()
            {
                StartOffset = start.TotalNanoSeconds(),
                ParentType = context.ObjectType.Name,
                FieldName = context.FieldName,
                Path = context.Path.Segments.ToList(),
                ReturnType = context.Field.Type.ToString()
            };
            context.Items.Add("trace", record);
            return default;
        }

        public ValueTask EndResolveAsync(IResolverContext context, IResolverResult result)
        {
            var end = _stopwatch.Elapsed.TotalNanoSeconds();

            if (context.Items.TryGetValue("trace", out var recordItem))
            {

                if (!(recordItem is TraceExtensionRecord.ResolverTrace record))
                    return default;

                record.Duration = (end - record.StartOffset);
                _resolverTraces.Add(record);
            }

            return default;
        }
    }
}
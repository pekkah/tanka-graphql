using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private Func<DateTime> _utcNow;

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
            var endTime = _startTime.Add(_stopwatch.Elapsed);
            var duration = ToNanoSeconds((endTime - _startTime).TotalMilliseconds);

            var record = new TraceExtensionRecord()
            {
                Duration = duration,
                StartTime = _startTime,
                EndTime = endTime,
                Parsing = new TraceExtensionRecord.OperationTrace()
                {
                    StartOffset = ToNanoSeconds((_parsingStarted).TotalMilliseconds),
                    Duration = ToNanoSeconds((_parsingEnded - _parsingStarted).TotalMilliseconds)
                },
                Validation = new TraceExtensionRecord.OperationTrace()
                {
                    StartOffset = ToNanoSeconds((_validationStarted).TotalMilliseconds),
                    Duration = ToNanoSeconds((_validationEnded - _validationStarted).TotalMilliseconds)
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


        protected static long ToNanoSeconds(double ms) => (long)(ms * 1000 * 1000);
    }
}
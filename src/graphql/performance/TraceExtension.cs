using System;
using System.Diagnostics;
using System.Threading.Tasks;
using fugu.graphql.validation;

namespace fugu.graphql.performance
{
    public class TraceExtension : ExtensionBase
    {
        private DateTime _startTime;
        private Stopwatch _stopwatch;
        private TimeSpan _validationStarted;
        private TimeSpan _validationEnded;

        public override Task BeginExecuteAsync(ExecutionOptions options)
        {
            _startTime = DateTime.UtcNow;
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
                Validation = new TraceExtensionRecord.OperationTrace()
                {
                    StartOffset = ToNanoSeconds((_validationStarted).TotalMilliseconds),
                    Duration = ToNanoSeconds((_validationEnded - _validationStarted).TotalMilliseconds)
                }
            };

            executionResult.AddExtension("tracing", record);
            return Task.CompletedTask;
        }

        public static long ToNanoSeconds(double ms) => (long)(ms * 1000 * 1000);
    }
}
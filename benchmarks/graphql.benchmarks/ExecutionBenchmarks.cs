using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class ExecutionBenchmarks
    {
        private ExecutableDocument _query;
        private ISchema _schema;
        private ExecutableDocument _mutation;
        private ExecutableDocument _subscription;

        [GlobalSetup]
        public void Setup()
        {
            _schema = Utils.InitializeSchema();
            _query = Utils.InitializeQuery();
            _mutation = Utils.InitializeMutation();
            _subscription = Utils.InitializeSubscription();
        }

        [Benchmark(Baseline = true)]
        public async Task Query_without_validation()
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _query,
                Schema = _schema,
                Validate = null
            });

            AssertResult(result.Errors);
        }
        
        [Benchmark]
        public async Task Query_with_defaults()
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _query,
                Schema = _schema
            });

            AssertResult(result.Errors);
        }

        [Benchmark]
        public async Task Mutation_with_defaults()
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Document = _mutation,
                Schema = _schema
            });

            AssertResult(result.Errors);
        }

        [Benchmark]
        public async Task Mutation_without_validation()
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Document = _mutation,
                Schema = _schema,
                Validate = null
            });

            AssertResult(result.Errors);
        }

        [Benchmark]
        public async Task Subscribe_with_defaults()
        {
            var cts = new CancellationTokenSource();
            var result = await Executor.SubscribeAsync(new ExecutionOptions()
            {
                Document = _subscription,
                Schema = _schema
            }, cts.Token);

            AssertResult(result.Errors);
            cts.Cancel();
        }

        [Benchmark]
        public async Task Subscribe_without_validation()
        {
            var cts = new CancellationTokenSource();
            var result = await Executor.SubscribeAsync(new ExecutionOptions()
            {
                Document = _subscription,
                Schema = _schema,
                Validate = null
            }, cts.Token);

            AssertResult(result.Errors);
            cts.Cancel();
        }

        [Benchmark]
        public async Task Subscribe_with_defaults_and_get_value()
        {
            var cts = new CancellationTokenSource();
            var result = await Executor.SubscribeAsync(new ExecutionOptions()
            {
                Document = _subscription,
                Schema = _schema
            }, cts.Token);

            AssertResult(result.Errors);

            var value = await result.Source.Reader.ReadAsync(cts.Token);
            AssertResult(value.Errors);
            cts.Cancel();
        }

        private static void AssertResult(IEnumerable<ExecutionError> errors)
        {
            if (errors != null && errors.Any())
            {
                throw new InvalidOperationException(
                    $"Execution failed. {string.Join("", errors.Select(e => e.Message))}");
            }
        }
    }
}
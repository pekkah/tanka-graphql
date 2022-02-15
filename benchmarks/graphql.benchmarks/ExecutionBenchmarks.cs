using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Benchmarks;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
public class ExecutionBenchmarks
{
    private ExecutableDocument _mutation;
    private ExecutableDocument _query;
    private ISchema _schema;
    private ExecutableDocument _subscription;

    [Params(1)] public int ExecutionCount { get; set; } = 1;

    [Benchmark]
    public async Task Mutation_with_defaults()
    {
        for (var i = 0; i < ExecutionCount; i++)
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _mutation,
                Schema = _schema
            });

            AssertResult(result.Errors);
        }
    }

    [Benchmark]
    public async Task Mutation_without_validation()
    {
        for (var i = 0; i < ExecutionCount; i++)
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _mutation,
                Schema = _schema,
                Validate = null
            });

            AssertResult(result.Errors);
        }
    }

    [Benchmark]
    public async Task Query_with_defaults()
    {
        for (var i = 0; i < ExecutionCount; i++)
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _query,
                Schema = _schema
            });

            AssertResult(result.Errors);
        }
    }

    [Benchmark(Baseline = true)]
    public async Task Query_without_validation()
    {
        for (var i = 0; i < ExecutionCount; i++)
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _query,
                Schema = _schema,
                Validate = null
            });

            AssertResult(result.Errors);
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        _schema = Utils.InitializeSchema().Result;
        _query = Utils.InitializeQuery();
        _mutation = Utils.InitializeMutation();
        _subscription = Utils.InitializeSubscription();
    }

    [Benchmark]
    public async Task Subscribe_with_defaults()
    {
        for (var i = 0; i < ExecutionCount; i++)
        {
            var cts = new CancellationTokenSource();
            var result = await Executor.SubscribeAsync(new ExecutionOptions
            {
                Document = _subscription,
                Schema = _schema
            }, cts.Token);

            AssertResult(result.Errors);
            cts.Cancel();
        }
    }

    [Benchmark]
    public async Task Subscribe_with_defaults_and_get_value()
    {
        for (var i = 0; i < ExecutionCount; i++)
        {
            var cts = new CancellationTokenSource();
            var result = await Executor.SubscribeAsync(new ExecutionOptions
            {
                Document = _subscription,
                Schema = _schema
            }, cts.Token);

            AssertResult(result.Errors);

            var value = await result.Source.Reader.ReadAsync(cts.Token);
            AssertResult(value.Errors);
            cts.Cancel();
        }
    }

    [Benchmark]
    public async Task Subscribe_without_validation()
    {
        for (var i = 0; i < ExecutionCount; i++)
        {
            var cts = new CancellationTokenSource();
            var result = await Executor.SubscribeAsync(new ExecutionOptions
            {
                Document = _subscription,
                Schema = _schema,
                Validate = null
            }, cts.Token);

            AssertResult(result.Errors);
            cts.Cancel();
        }
    }

    private static void AssertResult(IEnumerable<ExecutionError> errors)
    {
        if (errors != null && errors.Any())
            throw new InvalidOperationException(
                $"Execution failed. {string.Join("", errors.Select(e => e.Message))}");
    }
}
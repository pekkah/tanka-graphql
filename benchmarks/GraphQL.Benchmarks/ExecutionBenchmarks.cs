using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Logging.Abstractions;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Benchmarks.Experimental;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
//[MarkdownExporterAttribute.GitHub]
public class ExecutionBenchmarks
{
    private ExecutableDocument _complexQuery;
    private Executor _executor;
    private ExecutableDocument _mutation;
    private ExecutableDocument _query;
    private ISchema _schema;
    private ExecutableDocument _subscription;
    private Executor _nonValidatingExecutor;

    [Benchmark]
    public async Task Mutation_with_defaults()
    {
        var result = await _executor.Execute(new GraphQLRequest(_mutation));

        AssertResult(result.Errors);
    }

    [Benchmark]
    public async Task Mutation_without_validation()
    {
        var result = await _nonValidatingExecutor.Execute(new GraphQLRequest(_mutation));

        AssertResult(result.Errors);
    }

    [Benchmark]
    public async Task Query_with_validation()
    {
        var result = await _executor.Execute(new GraphQLRequest(_query));

        AssertResult(result.Errors);
    }

    [Benchmark(Baseline = true)]
    public async Task Query_without_validation()
    {
        var result = await _nonValidatingExecutor.Execute(new GraphQLRequest(_query));

        AssertResult(result.Errors);
    }

    [Benchmark]
    public async Task Query_Complex_with_validation()
    {
        var result = await _executor.Execute(new GraphQLRequest(_complexQuery));

        AssertResult(result.Errors);
    }

    [Benchmark]
    public async Task Query_Complex_without_validation()
    {
        var result = await _nonValidatingExecutor.Execute(new GraphQLRequest(_complexQuery));

        AssertResult(result.Errors);
    }

    [GlobalSetup]
    public void Setup()
    {
        _schema = Utils.InitializeSchema().Result;
        _query = Utils.InitializeQuery();
        _complexQuery = Utils.InitializeComplexQuery();
        _mutation = Utils.InitializeMutation();
        _subscription = Utils.InitializeSubscription();
        _executor = new Executor(_schema);
        _nonValidatingExecutor = new Executor(new ExecutorOptions()
        {
            Schema = _schema,
            ValidationEnabled = false
        });
    }

    [Benchmark]
    public async Task Subscribe_with_defaults()
    {
        var cts = new CancellationTokenSource();
        var result = await _executor.Subscribe(new GraphQLRequest()
        {
            Query = _subscription,
        }, cts.Token).SingleAsync(cancellationToken: cts.Token);

        AssertResult(result.Errors);
        cts.Cancel();
    }

    [Benchmark]
    public async Task Subscribe_without_validation()
    {
        var cts = new CancellationTokenSource();
        var result = await _nonValidatingExecutor.Subscribe(new GraphQLRequest()
        {
            Query = _subscription,
        }, cts.Token).SingleAsync(cancellationToken: cts.Token);

        AssertResult(result.Errors);
        cts.Cancel();
    }

    private static void AssertResult(IEnumerable<ExecutionError> errors)
    {
        if (errors is not null)
        {
            var list = errors.ToList();

            if (list.Count > 0)
            {
                var json = JsonSerializer.Serialize(list);
                throw new InvalidOperationException(
                    $"Execution failed\n: {json}");
            }
        }
    }
}
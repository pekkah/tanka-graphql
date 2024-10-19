using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

namespace Tanka.GraphQL.Benchmarks;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class PerformanceBenchmarks
{
    private ExecutableDocument _queryWithDeeplyNestedFields;
    private ExecutableDocument _mutationWithLargeArray;
    private ExecutableDocument _subscriptionWithHighThroughput;
    private Executor _executor;
    private ISchema _schema;

    [Benchmark]
    public async Task Query_with_deeply_nested_fields()
    {
        var result = await _executor.Execute(new GraphQLRequest(_queryWithDeeplyNestedFields));

        AssertResult(result.Errors);
    }

    [Benchmark]
    public async Task Mutation_with_large_array()
    {
        var result = await _executor.Execute(new GraphQLRequest(_mutationWithLargeArray));

        AssertResult(result.Errors);
    }

    [Benchmark]
    public async Task Subscription_with_high_throughput()
    {
        var result = await _executor.Execute(new GraphQLRequest(_subscriptionWithHighThroughput));

        AssertResult(result.Errors);
    }

    [GlobalSetup]
    public void Setup()
    {
        _schema = Utils.InitializeSchema().Result;
        _queryWithDeeplyNestedFields = Utils.InitializeQueryWithDeeplyNestedFields();
        _mutationWithLargeArray = Utils.InitializeMutationWithLargeArray();
        _subscriptionWithHighThroughput = Utils.InitializeSubscriptionWithHighThroughput();
        _executor = new Executor(_schema);
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

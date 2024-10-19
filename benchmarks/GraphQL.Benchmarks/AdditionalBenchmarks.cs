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
public class AdditionalBenchmarks
{
    private ExecutableDocument _complexQueryWithFragments;
    private ExecutableDocument _mutationWithNestedInputs;
    private ExecutableDocument _subscriptionWithLargePayload;
    private Executor _executor;
    private ISchema _schema;

    [Benchmark]
    public async Task ComplexQuery_with_multiple_fragments()
    {
        var result = await _executor.Execute(new GraphQLRequest(_complexQueryWithFragments));

        AssertResult(result.Errors);
    }

    [Benchmark]
    public async Task Mutation_with_nested_inputs()
    {
        var result = await _executor.Execute(new GraphQLRequest(_mutationWithNestedInputs));

        AssertResult(result.Errors);
    }

    [Benchmark]
    public async Task Subscription_with_large_payload()
    {
        var result = await _executor.Execute(new GraphQLRequest(_subscriptionWithLargePayload));

        AssertResult(result.Errors);
    }

    [GlobalSetup]
    public void Setup()
    {
        _schema = Utils.InitializeSchema().Result;
        _complexQueryWithFragments = Utils.InitializeComplexQueryWithFragments();
        _mutationWithNestedInputs = Utils.InitializeMutationWithNestedInputs();
        _subscriptionWithLargePayload = Utils.InitializeSubscriptionWithLargePayload();
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

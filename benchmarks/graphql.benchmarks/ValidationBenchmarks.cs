using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class ValidationBenchmarks
    {
        private ISchema _schema;
        private List<CombineRule> _defaultRulesMap;
        private GraphQLDocument _query;

        [GlobalSetup]
        public void Setup()
        {
            _schema = Utils.InitializeSchema();
            _query = Utils.InitializeQuery();
            _defaultRulesMap = ExecutionRules.All.ToList();
        }

        [Benchmark]
        public void Validate_with_defaults()
        {
            var result = Validator.Validate(
                _defaultRulesMap,
                _schema,
                _query);

            if (!result.IsValid)
            {
                throw new InvalidOperationException(
                    $"Validation failed. {result}");
            }
        }
    }
}
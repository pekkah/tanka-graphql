using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Validation;
using Tanka.GraphQL.Language.Visitors;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class ValidationBenchmarks
    {
        private List<CombineRule> _comparisonRules;
        private List<CombineRule> _defaultRulesMap;
        private ExecutableDocument _query;
        private ExecutableDocument _query2;
        private ISchema _schema;

        [GlobalSetup]
        public void Setup()
        {
            _schema = Utils.InitializeSchema();
            _query = Utils.InitializeQuery();
            _defaultRulesMap = ExecutionRules.All.ToList();
            _comparisonRules = new List<CombineRule>
            {
                ExecutionRules.R5211OperationNameUniqueness()
            };

            _query2 = @"query Q1 { field1, field2, field3 { field1 field2 }} query Q2 { field1 }";
        }

        [Benchmark(Baseline = true)]
        public void Tanka_1()
        {
            var result = Validator.Validate(
                _comparisonRules,
                _schema,
                _query);

            if (!result.IsValid)
                throw new InvalidOperationException(
                    $"Validation failed. {result}");
        }

        [Benchmark(Baseline = false)]
        public void Tanka_1_1000()
        {
            for (var i = 0; i < 1000; i++)
            {
                var result = Validator.Validate(
                    _comparisonRules,
                    _schema,
                    _query);

                if (!result.IsValid)
                    throw new InvalidOperationException(
                        $"Validation failed. {result}");
            }
        }

        [Benchmark]
        public void Tanka_2()
        {
            var rule = new  OperationNameUniquenessRule();
            var walker = new ReadOnlyExecutionDocumentWalker(
                new ExecutionDocumentWalkerOptions()
                    {
                        ExecutableDocument =
                        {
                            rule
                        },
                        OperationDefinition =
                        {
                            rule
                        }
                    }
                );

            walker.Visit(_query2);
        }

        [Benchmark]
        public void Tanka_2__1000()
        {
            for (var i = 0; i < 1000; i++)
            {
                var rule = new  OperationNameUniquenessRule();
                var walker = new ReadOnlyExecutionDocumentWalker(
                    new ExecutionDocumentWalkerOptions()
                    {
                        ExecutableDocument =
                        {
                            rule
                        },
                        OperationDefinition =
                        {
                            rule
                        }
                    }
                );

                walker.Visit(_query2);
            }
        }

        //[Benchmark]
        public void Validate_with_defaults()
        {
            var result = Validator.Validate(
                _defaultRulesMap,
                _schema,
                _query);

            if (!result.IsValid)
                throw new InvalidOperationException(
                    $"Validation failed. {result}");
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BenchmarkDotNet.Attributes;
using tanka.graphql.type;
using tanka.graphql.validation;
using GraphQLParser.AST;

namespace tanka.graphql.benchmarks
{
    [CoreJob]
    //[ClrJob]
    [MarkdownExporterAttribute.GitHub()]
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private GraphQLDocument _query;
        private ISchema _schema;
        private GraphQLDocument _mutation;
        private GraphQLDocument _subscription;
        private IEnumerable<CombineRule> _defaultRulesMap;

        [GlobalSetup]
        public async Task Setup()
        {
            _schema = await Utils.InitializeSchema();
            _query = Utils.InitializeQuery();
            _mutation = Utils.InitializeMutation();
            _subscription = Utils.InitializeSubscription();
            _defaultRulesMap = ExecutionRules.All;
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
        public async Task Query_without_validation()
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _query,
                Schema = _schema,
                Validate = false
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
                Validate = false
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
        }

        [Benchmark]
        public async Task Subscribe_without_validation()
        {
            var cts = new CancellationTokenSource();
            var result = await Executor.SubscribeAsync(new ExecutionOptions()
            {
                Document = _subscription,
                Schema = _schema,
                Validate = false
            }, cts.Token);

            AssertResult(result.Errors);
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

            var value = result.Source.Receive();
            AssertResult(value.Errors);
        }
        
        [Benchmark]
        public void Validate_query_with_defaults()
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

        private static void AssertResult(IEnumerable<Error> errors)
        {
            if (errors != null && errors.Any())
            {
                throw new InvalidOperationException(
                    $"Execution failed. {string.Join("", errors.Select(e => e.Message))}");
            }
        }
    }
}
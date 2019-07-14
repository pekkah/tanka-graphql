using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using tanka.graphql.type;
using tanka.graphql.validation;
using GraphQLParser.AST;
using tanka.graphql.resolvers;

namespace tanka.graphql.benchmarks
{
    [MarkdownExporterAttribute.GitHub()]
    public class Benchmarks
    {
        private GraphQLDocument _query;
        private ISchema _schema;
        private GraphQLDocument _mutation;
        private GraphQLDocument _subscription;
        private IEnumerable<CombineRule> _defaultRulesMap;
        private Resolver _resolverChain;
        private Resolver _resolver;

        [GlobalSetup]
        public void Setup()
        {
            _schema = Utils.InitializeSchema();
            _query = Utils.InitializeQuery();
            _mutation = Utils.InitializeMutation();
            _subscription = Utils.InitializeSubscription();
            _defaultRulesMap = ExecutionRules.All;
            _resolverChain = new ResolverBuilder()
                .Use((context, next) => next(context))
                .Use((context, next) => next(context))
                .Run(context => new ValueTask<IResolveResult>(Resolve.As(42)))
                .Build();

            _resolver = context => new ValueTask<IResolveResult>(Resolve.As(42));
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
                Validate = null
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

        [Benchmark]
        public void Validate_100times_with_defaults()
        {
            for (int i = 0; i < 100; i++)
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
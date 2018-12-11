using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using fugu.graphql;
using fugu.graphql.resolvers;
using fugu.graphql.tools;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace graphql.benchmarks
{
    public class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }

    [CoreJob]
    //[ClrJob]
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private GraphQLDocument _query;
        private ISchema _schema;

        [GlobalSetup]
        public async Task Setup()
        {
            _schema = await Utils.InitializeSchema();
            _query = Utils.InitializeQuery();
        }

        [Benchmark]
        public Task Query_with_defaults()
        {
            return Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _query,
                Schema = _schema
            });
        }

        [Benchmark]
        public Task Query_without_validation()
        {
            return Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = _query,
                Schema = _schema,
                Validate = false
            });
        }
    }

    public static class Utils
    {
        public static Task<ISchema> InitializeSchema()
        {
            var query = new ObjectType("Query", new Fields
            {
                {"simple", new Field(ScalarType.String)}
            });

            var resolvers = new ResolverMap
            {
                {
                    "Query", new FieldResolverMap
                    {
                        {"simple", context => Task.FromResult(Resolve.As("value"))}
                    }
                }
            };

            var schema = SchemaTools.MakeExecutableSchemaAsync(
                new Schema(query), resolvers);

            return schema;
        }

        public static GraphQLDocument InitializeQuery()
        {
            return Parser.ParseDocument(@"
{
    simple
}");
        }
    }
}
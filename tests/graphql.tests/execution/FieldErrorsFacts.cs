using System.ComponentModel;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.sdl;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.execution
{
    public class Query
    {
        public Query()
        {
            Container = new Container();
        }

        public Container Container { get; }
    }

    public class Container
    {
        public string NonNull_WithNull => null;

        public string NonNull => "value";
    }

    public class FieldErrorsFacts
    {
        private ISchema _schema;

        public Query Query { get; }

        public FieldErrorsFacts()
        {
            Query = new Query();
            var builder = new SchemaBuilder();
            Sdl.Import(Parser.ParseDocument(
                @"
                    type Container {
                        nonNullWithNull: String!
                    }

                    type Query {
                        container: Container
                    }

                    schema {
                        query : Query
                    }
                "), builder);

            var resolvers = new ResolverMap()
            {
                ["Container"] = new FieldResolverMap()
                {
                    {"nonNullWithNull", Resolve.PropertyOf<Container>(c => c.NonNull_WithNull)}
                },
                ["Query"] = new FieldResolverMap()
                {
                    {"container" , context => new ValueTask<IResolveResult>(Resolve.As(Query))}
                }
            };

            _schema = SchemaTools.MakeExecutableSchema(
                builder,
                resolvers);
        }

        [Fact]
        public async Task NullValue_resolved_for_non_null_field()
        {
            /* Given */
            var query = Parser.ParseDocument(
                @"
                {
                    container {
                        nonNullWithNull
                    }
                }
                ");


            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
                Document = query
            });

            /* Then */
            result.ShouldMatchJson(@"{}");
        }
    }
}
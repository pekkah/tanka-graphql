using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Extensions.Trace;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.Tracing.Tests
{
    public class TraceFacts
    {
        public TraceFacts()
        {
            Schema = new ExecutableSchemaBuilder()
                .Add("Query", new()
                {
                    ["simple: String!"] = b => b.ResolveAs("string")
                })
                .Build().Result; ;
        }

        public ISchema Schema { get; set; }

        [Fact]
        public async Task Test1()
        {
            /* Given */
            var executor = new Executor(new ExecutorOptions()
            {
                Schema = Schema,
                TraceEnabled = true
            });

            /* When */
            var response = await executor.Execute(new GraphQLRequest("{ simple }"));


            /* Then */
            var trace = response.Extensions?["trace"] as TraceExtension;
            Assert.NotNull(trace);

            Assert.True(trace.Elapsed > 0);
        }
    }
}
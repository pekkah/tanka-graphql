using System.Threading.Tasks;
using Tanka.GraphQL.Language;
using Xunit;

namespace Tanka.GraphQL.Experimental.Tests
{
    public class RequestPipelineFacts
    {
        [Fact]
        public async Task Execute()
        {
            /* Given */
            var pipeline = new RequestPipelineBuilder()
                .UseOperationSelector((context, options, token) =>
                {
                    context.Operation = options.Document.GetOperation(options.OperationName);
                    return Task.CompletedTask;
                })
                .Build();

            /* When */
            var result = await pipeline.ExecuteSingle(new RequestOptions());

            /* Then */
        }
    }
}
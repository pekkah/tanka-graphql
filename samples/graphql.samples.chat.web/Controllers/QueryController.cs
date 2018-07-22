using System.Threading.Tasks;
using fugu.graphql.samples.chat.web.GraphQL;
using Microsoft.AspNetCore.Mvc;
using static fugu.graphql.Executor;
using static fugu.graphql.Parser;

namespace fugu.graphql.samples.chat.web.Controllers
{
    [Route("api/graphql")]
    public class QueryController : Controller
    {
        private readonly ChatSchemas _schemas;

        public QueryController(ChatSchemas schemas)
        {
            _schemas = schemas;
        }

        [HttpPost]
        public async Task<IActionResult> Get([FromBody] OperationRequest request)
        {
            var result = await ExecuteAsync(new ExecutionOptions
            {
                Document = ParseDocument(request.Query),
                Schema = _schemas.Chat,
                OperationName = request.OperationName
            });

            return Ok(result);
        }
    }
}
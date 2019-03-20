using System.Threading.Tasks;
using tanka.graphql.samples.chat.web.GraphQL;
using tanka.graphql.server.utilities;
using Microsoft.AspNetCore.Mvc;
using static tanka.graphql.Executor;
using static tanka.graphql.Parser;

namespace tanka.graphql.samples.chat.web.Controllers
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
        public async Task<IActionResult> Post([FromBody] OperationRequest request)
        {
            var result = await ExecuteAsync(new ExecutionOptions
            {
                Document =  ParseDocument(request.Query),
                Schema = _schemas.Chat,
                OperationName = request.OperationName,
                VariableValues = request.Variables?.ToVariableDictionary()
            });

            return Ok(result);
        }
    }
}
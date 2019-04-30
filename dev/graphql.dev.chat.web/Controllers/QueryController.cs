using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.samples.chat.web.GraphQL;
using tanka.graphql.server.utilities;
using Microsoft.AspNetCore.Mvc;
using tanka.graphql.requests;
using static tanka.graphql.Executor;
using static tanka.graphql.Parser;

namespace tanka.graphql.samples.chat.web.Controllers
{
    [Route("api/graphql")]
    public class QueryController : Controller
    {
        private readonly ChatSchemas _schemas;
        private readonly IEnumerable<IExtension> _extensions;

        public QueryController(ChatSchemas schemas, IEnumerable<IExtension> extensions)
        {
            _schemas = schemas;
            _extensions = extensions;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OperationRequest request)
        {
            var result = await ExecuteAsync(new ExecutionOptions
            {
                Document =  ParseDocument(request.Query),
                Schema = _schemas.Chat,
                OperationName = request.OperationName,
                VariableValues = request.Variables?.ToNestedDictionary(),
                Extensions = _extensions.ToList()
            });

            return Ok(result);
        }
    }
}
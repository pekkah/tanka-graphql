using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tanka.graphql.requests;
using tanka.graphql.samples.chat.web.GraphQL;
using tanka.graphql.server;
using static tanka.graphql.Parser;

namespace tanka.graphql.samples.chat.web.Controllers
{
    [Route("api/graphql")]
    public class QueryController : Controller
    {
        private readonly IQueryStreamService _queryStreamService;

        public QueryController(IQueryStreamService queryStreamService)
        {
            _queryStreamService = queryStreamService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OperationRequest request)
        {
            var stream = await _queryStreamService.QueryAsync(new Query
            {
                Document = ParseDocument(request.Query),
                Variables = request.Variables.ToNestedDictionary(),
                OperationName = request.OperationName
            }, Request.HttpContext.RequestAborted);

            var result = await stream.Reader.ReadAsync();

            return Ok(result);
        }
    }
}
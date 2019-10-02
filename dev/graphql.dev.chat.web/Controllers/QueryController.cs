using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tanka.GraphQL.DTOs;
using Tanka.GraphQL.Samples.Chat.Web.GraphQL;
using Tanka.GraphQL.Server;
using static Tanka.GraphQL.Parser;

namespace Tanka.GraphQL.Samples.Chat.Web.Controllers
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
                Variables = request.Variables,
                OperationName = request.OperationName
            }, Request.HttpContext.RequestAborted);

            var result = await stream.Reader.ReadAsync();

            return Ok(result);
        }
    }
}
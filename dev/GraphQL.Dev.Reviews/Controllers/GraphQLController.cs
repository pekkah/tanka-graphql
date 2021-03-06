﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tanka.GraphQL;
using Tanka.GraphQL.Server;

namespace GraphQL.Dev.Reviews.Controllers
{
    [ApiController]
    [Route("graphql")]
    public class GraphQLController : ControllerBase
    {
        private readonly IQueryStreamService _query;
        private readonly ILogger<GraphQLController> _logger;

        public GraphQLController(
            IQueryStreamService query,
            ILogger<GraphQLController> logger)
        {
            _query = query;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ExecutionResult>> Get(OperationRequest operation)
        {
            var stream = await _query.QueryAsync(
                new Query()
                {
                    Document = Parser.ParseDocument(operation.Query),
                    Variables = operation.Variables,
                    OperationName = operation.OperationName
                }, HttpContext.RequestAborted);
            var result = await stream.Reader.ReadAsync(HttpContext.RequestAborted);

            return Ok(result);
        }
    }

    public class OperationRequest
    {
        public string OperationName { get; set; }

        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }
    }
}
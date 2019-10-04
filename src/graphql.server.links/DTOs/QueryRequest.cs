﻿using System.Collections.Generic;

namespace Tanka.GraphQL.Server.Links.DTOs
{
    public class QueryRequest
    {
        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> Extensions { get; set; }
    }
}
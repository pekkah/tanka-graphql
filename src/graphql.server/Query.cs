﻿using System.Collections.Generic;


namespace Tanka.GraphQL.Server
{
    public class Query
    {
        public ExecutableDocument Document { get; set; }

        public Dictionary<string, object> Variables { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> Extensions { get; set; }
    }
}
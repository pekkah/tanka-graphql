﻿using System;
using System.Threading.Tasks;
using tanka.graphql.requests;
using tanka.graphql.type;

namespace tanka.graphql.server
{
    public class ExecutionOptions
    {
        public Func<QueryRequest, ValueTask<ISchema>> GetSchema { get; set; }

        //public ISchema Schema { get; set; }
    }
}
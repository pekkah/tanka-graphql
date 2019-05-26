using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.requests;
using tanka.graphql.type;
using tanka.graphql.validation;

namespace tanka.graphql.server
{
    public class SchemaOptions
    {
        public Func<Query, ValueTask<ISchema>> GetSchema { get; set; }

        public IEnumerable<CombineRule> ValidationRules { get; set; }
    }
}
using System;
using System.Threading.Tasks;
using tanka.graphql.requests;
using tanka.graphql.type;

namespace tanka.graphql.server
{
    public class SchemaOptions
    {
        public Func<Query, ValueTask<ISchema>> GetSchema { get; set; }
    }
}
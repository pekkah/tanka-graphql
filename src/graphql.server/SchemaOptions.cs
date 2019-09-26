using System;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.DTOs;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Server
{
    public class SchemaOptions
    {
        public Func<Query, ValueTask<ISchema>> GetSchema { get; set; }

        public CombineRule[] ValidationRules { get; set; } = ExecutionRules.All.ToArray();
    }
}
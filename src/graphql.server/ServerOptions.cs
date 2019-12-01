using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Server
{
    public class ServerOptions
    {
        [Required(ErrorMessage = "GetSchema is required. Use 'AddTankaGraphQL().ConfigureSchema(..)' or one of its overloads")]
        public Func<Query, ValueTask<ISchema>> GetSchema { get; set; }

        public CombineRule[] ValidationRules { get; set; } = ExecutionRules.All.ToArray();
    }
}
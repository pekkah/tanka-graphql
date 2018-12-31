using System.Collections.Generic;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public interface IField : IDirectives
    {
        IGraphQLType Type { get; set; }

        IEnumerable<KeyValuePair<string, Argument>> Arguments { get; set; }

        Meta Meta { get; set; } 

        Resolver Resolve { get; set; }

        Subscriber Subscribe {get; set; }
    }
}
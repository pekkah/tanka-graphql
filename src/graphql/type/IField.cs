using System.Collections.Generic;
using fugu.graphql.resolvers;

namespace fugu.graphql.type
{
    public interface IField : IDirectives
    {
        IGraphQLType Type { get; }

        IEnumerable<KeyValuePair<string, Argument>> Arguments { get; }

        Meta Meta { get; set; } 

        Resolver Resolve { get; set; }

        Subscriber Subscribe {get; set; }
    }
}
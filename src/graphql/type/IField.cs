using System.Collections.Generic;

namespace fugu.graphql.type
{
    public interface IField : IDirectives
    {
        IGraphQLType Type { get; }

        IEnumerable<KeyValuePair<string, Argument>> Arguments { get; }

        Meta Meta { get; set; }
    }
}
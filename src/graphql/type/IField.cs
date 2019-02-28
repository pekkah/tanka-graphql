using System.Collections.Generic;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public interface IField : IDirectives, IDeprecable, IDescribable
    {
        IType Type { get; set; }

        IEnumerable<KeyValuePair<string, Argument>> Arguments { get; set; }

        Meta Meta { get; set; } 

        Resolver Resolve { get; set; }

        Subscriber Subscribe {get; set; }
        DirectiveInstance GetDirective(string name);
        Argument GetArgument(string name);
        bool HasArgument(string name);
    }
}
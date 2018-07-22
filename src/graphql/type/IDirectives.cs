using System.Collections.Generic;

namespace fugu.graphql.type
{
    public interface IDirectives
    {
        IEnumerable<DirectiveInstance> Directives { get; }

        DirectiveInstance GetDirective(string name);
    }
}
using System.Collections.Generic;

namespace tanka.graphql.type
{
    public interface IDirectives
    {
        IEnumerable<DirectiveInstance> Directives { get; }

        DirectiveInstance GetDirective(string name);
    }
}
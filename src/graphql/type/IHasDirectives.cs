using System.Collections.Generic;

namespace tanka.graphql.type
{
    public interface IHasDirectives
    {
        IEnumerable<DirectiveInstance> Directives { get; }

        DirectiveInstance GetDirective(string name);
    }
}
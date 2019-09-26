using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public interface IHasDirectives
    {
        IEnumerable<DirectiveInstance> Directives { get; }

        DirectiveInstance GetDirective(string name);

        bool HasDirective(string name);
    }
}
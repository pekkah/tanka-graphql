using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem;

public interface IHasDirectives
{
    IEnumerable<Directive> Directives { get; }

    Directive? GetDirective(string name);

    bool HasDirective(string name);
}
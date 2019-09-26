using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Directives
{
    public delegate T DirectiveNodeVisitor<T>(DirectiveInstance directive, T node);
}
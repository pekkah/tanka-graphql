using tanka.graphql.type;

namespace tanka.graphql.directives
{
    public delegate T DirectiveNodeVisitor<T>(DirectiveInstance directive, T node);
}
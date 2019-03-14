using tanka.graphql.type;

namespace tanka.graphql.tools
{
    public delegate T DirectiveNodeVisitor<T>(DirectiveInstance directive, T node);
}
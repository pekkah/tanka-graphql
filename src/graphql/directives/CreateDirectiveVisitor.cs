using tanka.graphql.schema;
using tanka.graphql.tools;

namespace tanka.graphql.directives
{
    public delegate DirectiveVisitor CreateDirectiveVisitor(SchemaBuilder builder);
}
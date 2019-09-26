using Tanka.GraphQL.SchemaBuilding;

namespace Tanka.GraphQL.Directives
{
    public delegate DirectiveVisitor CreateDirectiveVisitor(SchemaBuilder builder);
}
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate ObjectDefinition ResolveAbstractType(ExecutableSchema schema, TypeDefinition abstractType, object value);
}
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public delegate ObjectDefinition ResolveAbstractType(ExecutableSchema schema, TypeDefinition abstractType, object value);
}
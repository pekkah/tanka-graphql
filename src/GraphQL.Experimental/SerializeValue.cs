using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public delegate ValueTask<object?> SerializeValue(
        ExecutableSchema schema, 
        TypeDefinition typeDefinition,
        object? value
        );
}
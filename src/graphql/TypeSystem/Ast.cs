
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem
{
    public static class Ast
    {
        public static IType TypeFromAst(ISchema schema, TypeBase type)
        {
            if (type == null)
                return null;

            if (type.Kind == NodeKind.NonNullType)
            {
                var innerType = TypeFromAst(schema, ((NonNullType)type).OfType);
                return new NonNull(innerType);
            }

            if (type.Kind == NodeKind.ListType)
            {
                var innerType = TypeFromAst(schema, ((ListType)type).OfType);
                return new List(innerType);
            }

            if (type.Kind == NodeKind.NamedType)
            {
                var namedType = (NamedType) type;
                return schema.GetNamedType(namedType.Name);
            }

            throw new DocumentException(
                $"Unexpected type kind: {type.Kind}");
        }
    }
}
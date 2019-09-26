using GraphQLParser.AST;
using Tanka.GraphQL.Language;

namespace Tanka.GraphQL.TypeSystem
{
    public static class Ast
    {
        public static IType TypeFromAst(ISchema schema, GraphQLType type)
        {
            if (type == null)
                return null;

            if (type.Kind == ASTNodeKind.NonNullType)
            {
                var innerType = TypeFromAst(schema, ((GraphQLNonNullType)type).Type);
                return innerType != null ? new NonNull(innerType) : null;
            }

            if (type.Kind == ASTNodeKind.ListType)
            {
                var innerType = TypeFromAst(schema, ((GraphQLListType)type).Type);
                return innerType != null ? new List(innerType) : null;
            }

            if (type.Kind == ASTNodeKind.NamedType)
            {
                var namedType = (GraphQLNamedType) type;
                return schema.GetNamedType(namedType.Name.Value);
            }

            throw new DocumentException(
                $"Unexpected type kind: {type.Kind}");
        }
    }
}
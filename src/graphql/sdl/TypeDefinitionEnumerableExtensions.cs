using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace tanka.graphql.sdl
{
    public static class TypeDefinitionEnumerableExtensions
    {
        private static readonly IReadOnlyCollection<ASTNodeKind> InputTypeKinds = new List<ASTNodeKind>
        {
            ASTNodeKind.InputObjectTypeDefinition,
            ASTNodeKind.ScalarTypeDefinition,
            ASTNodeKind.EnumTypeDefinition
        };


        public static GraphQLTypeDefinition InputType(
            this IEnumerable<ASTNode> definitions,
            string name)
        {
            foreach (var inputDefinition in definitions.Where(def => InputTypeKinds.Contains(def.Kind)))
                switch (inputDefinition)
                {
                    case GraphQLInputObjectTypeDefinition inputObject when inputObject.Name.Value == name:
                        return inputObject;
                    case GraphQLScalarTypeDefinition scalarType when scalarType.Name.Value == name:
                        return scalarType;
                    case GraphQLEnumTypeDefinition enumType when enumType.Name.Value == name:
                        return enumType;
                }

            return null;
        }

        public static ASTNode OutputType(
            this IEnumerable<ASTNode> definitions,
            string name)
        {
            foreach (var inputDefinition in definitions)
                switch (inputDefinition)
                {
                    case GraphQLInputObjectTypeDefinition inputObject when inputObject.Name.Value == name:
                        return inputObject;
                    case GraphQLScalarTypeDefinition scalarType when scalarType.Name.Value == name:
                        return scalarType;
                    case GraphQLEnumTypeDefinition enumType when enumType.Name.Value == name:
                        return enumType;
                    case GraphQLObjectTypeDefinition objectType when objectType.Name.Value == name:
                        return objectType;
                    case GraphQLInterfaceTypeDefinition interfaceType when interfaceType.Name.Value == name:
                        return interfaceType;
                    case GraphQLUnionTypeDefinition unionType when unionType.Name.Value == name:
                        return unionType;
                }

            return null;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;


namespace Tanka.GraphQL.SDL
{
    public static class TypeDefinitionEnumerableExtensions
    {
        private static readonly IReadOnlyCollection<NodeKind> InputTypeKinds = new List<NodeKind>
        {
            NodeKind.InputObjectDefinition,
            NodeKind.ScalarDefinition,
            NodeKind.EnumDefinition
        };


        public static TypeDefinition? InputType(
            this IEnumerable<TypeDefinition> definitions,
            string name)
        {
            foreach (var inputDefinition in definitions.Where(def => InputTypeKinds.Contains(def.Kind)))
                switch (inputDefinition)
                {
                    case InputObjectDefinition inputObject when inputObject.Name == name:
                        return inputObject;
                    case ScalarDefinition scalarType when scalarType.Name == name:
                        return scalarType;
                    case EnumDefinition enumType when enumType.Name == name:
                        return enumType;
                }

            return null;
        }

        public static TypeDefinition OutputType(
            this IEnumerable<TypeDefinition> definitions,
            string name)
        {
            foreach (var inputDefinition in definitions)
                switch (inputDefinition)
                {
                    case InputObjectDefinition inputObject when inputObject.Name == name:
                        return inputObject;
                    case ScalarDefinition scalarType when scalarType.Name == name:
                        return scalarType;
                    case EnumDefinition enumType when enumType.Name == name:
                        return enumType;
                    case ObjectDefinition objectType when objectType.Name == name:
                        return objectType;
                    case InterfaceDefinition interfaceType when interfaceType.Name == name:
                        return interfaceType;
                    case UnionDefinition unionType when unionType.Name == name:
                        return unionType;
                }

            return null;
        }
    }
}
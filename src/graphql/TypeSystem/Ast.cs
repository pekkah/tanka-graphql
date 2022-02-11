using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem
{
    public static class Ast
    {
        public static bool GetIfArgumentValue(
            Directive directive,
            IReadOnlyDictionary<string, object?>? coercedVariableValues,
            Argument argument)
        {
            if (directive == null) throw new ArgumentNullException(nameof(directive));
            if (argument == null) throw new ArgumentNullException(nameof(argument));

            switch (argument.Value)
            {
                case { Kind: NodeKind.BooleanValue }:
                    return ((BooleanValue)argument.Value).Value;
                case { Kind: NodeKind.Variable }:
                    var variable = (Variable)argument.Value;
                    var variableValue = coercedVariableValues?[variable.Name];

                    if (variableValue == null)
                        throw new Exception(
                            $"If argument of {directive} is null. Variable value should not be null");

                    return (bool)variableValue;
                default:
                    return false;
            }
        }

        public static bool DoesFragmentTypeApply(
            ObjectDefinition objectType,
            TypeDefinition fragmentType)
        {
            if (objectType.Name == fragmentType.Name)
                return true;

            return fragmentType switch
            {
                InterfaceDefinition interfaceType => objectType.HasInterface(interfaceType.Name),
                UnionDefinition unionType => unionType.HasMember(objectType.Name),
                _ => false
            };
        }

        public static TypeDefinition? UnwrapAndResolveType(ISchema schema, TypeBase? type)
        {
            return type switch
            {
                null => null,
                NonNullType nonNullType => UnwrapAndResolveType(schema, nonNullType.OfType),
                ListType list => UnwrapAndResolveType(schema, list.OfType),
                NamedType namedType => schema.GetNamedType(namedType.Name),
                _ => throw new InvalidOperationException($"Unsupported type '{type}'")
            };
        }
    }
}
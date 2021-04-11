using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public static class Serialize
    {
        public static ValueTask<object?> SerializeValue(
            ExecutableSchema schema,
            object? value,
            TypeDefinition typeDefinition,
            IReadOnlyDictionary<string, SerializeValue> valueSerializers)
        {
            if (!valueSerializers.TryGetValue(typeDefinition.Name, out var converter))
                throw new InvalidOperationException(
                    $"Cannot serialize value. No value serializer given for type '{typeDefinition.Name}'.");

            try
            {
                return converter(schema, typeDefinition, value);
            }
            catch (Exception x)
            {
                // wrap exceptions in SerializationException if needed
                if (x is not SerializationException)
                    throw new SerializationException(
                        $"Could not serialize value '{typeDefinition.ToGraphQL()}' of type '{typeDefinition.Name}'. " +
                        "Serializing value resulted in error.", x);

                throw;
            }
        }
    }
}
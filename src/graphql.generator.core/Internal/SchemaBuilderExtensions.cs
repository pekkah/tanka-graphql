using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.TypeSystem;

// ReSharper disable CheckNamespace

namespace Tanka.GraphQL.SchemaBuilding
{
    internal static class SchemaBuilderExtensions
    {
        public static IEnumerable<KeyValuePair<string, IField>> GetFields(
            this SchemaBuilder builder, ComplexType objectType)
        {
            var fields = new List<KeyValuePair<string, IField>>();
            builder.Connections(connections =>
                fields = connections.GetFields(objectType).ToList());

            return fields;
        }

        public static List<KeyValuePair<string, InputObjectField>> GetInputFields(
            this SchemaBuilder builder, InputObjectType inputObjectType)
        {
            var fields = new List<KeyValuePair<string, InputObjectField>>();
            builder.Connections(connections =>
                fields = connections.GetInputFields(inputObjectType).ToList());

            return fields;
        }

        public static bool IsSubscriptionType(this SchemaBuilder builder, ComplexType objectType)
        {
            return builder.SubscriptionTypeName == objectType.Name;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class SchemaBuilder
    {
        public ISchema Build()
        {
            var (fields, inputFields, resolvers, subscribers) = _connections.Build();
            return new SchemaGraph(
                _types,
                fields,
                inputFields,
                _directives,
                BuildResolvers(resolvers),
                BuildSubscribers(subscribers),
                _scalarSerializers,
                _queryTypeName,
                _mutationTypeName,
                _subscriptionTypeName,
                _schemaDirectives);
        }

        public (ISchema Schema, object ValidationResult) BuildAndValidate()
        {
            var schema = Build();
            return (schema, new NotImplementedException("todo"));
        }

        private IReadOnlyDictionary<string, Dictionary<string, Subscriber>> BuildSubscribers(
            IReadOnlyDictionary<string, Dictionary<string, SubscriberBuilder>> subscribers)
        {
            var result = new Dictionary<string, Dictionary<string, Subscriber>>();

            foreach (var type in subscribers)
                result[type.Key] = type.Value.Select(f => (f.Key, f.Value.Build()))
                    .ToDictionary(f => f.Key, f => f.Item2);

            return result;
        }

        private IReadOnlyDictionary<string, Dictionary<string, Resolver>> BuildResolvers(
            IReadOnlyDictionary<string, Dictionary<string, ResolverBuilder>> resolvers)
        {
            var result = new Dictionary<string, Dictionary<string, Resolver>>();

            foreach (var type in resolvers)
                result[type.Key] = type.Value.Select(f => (f.Key, f.Value.Build()))
                    .ToDictionary(f => f.Key, f => f.Item2);

            return result;
        }
    }
}
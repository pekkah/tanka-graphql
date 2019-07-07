using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.schema
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
                BuildSubscribers(subscribers));
        }

        public (ISchema Schema, object ValidationResult) BuildAndValidate()
        {
            var schema = Build();
            return (schema, new NotImplementedException("todo"));
        }

        private Dictionary<string, Dictionary<string, Subscriber>> BuildSubscribers(
            Dictionary<string, Dictionary<string, SubscriberBuilder>> subscribers)
        {
            var result = new Dictionary<string, Dictionary<string, Subscriber>>();

            foreach (var type in subscribers)
                result[type.Key] = type.Value.Select(f => (f.Key, f.Value.Build()))
                    .ToDictionary(f => f.Key, f => f.Item2);

            return result;
        }

        private Dictionary<string, Dictionary<string, Resolver>> BuildResolvers(
            Dictionary<string, Dictionary<string, ResolverBuilder>> resolvers)
        {
            var result = new Dictionary<string, Dictionary<string, Resolver>>();

            foreach (var type in resolvers)
                result[type.Key] = type.Value.Select(f => (f.Key, f.Value.Build()))
                    .ToDictionary(f => f.Key, f => f.Item2);

            return result;
        }
    }
}
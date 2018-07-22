using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.type;

namespace fugu.graphql
{
    public class ResolverMap : Dictionary<string, FieldResolverMap>, IResolverMap, ISubscriberMap
    {
        public Task<Resolver> GetResolverAsync(ResolverContext resolverContext)
        {
            var objectType = resolverContext.ObjectType;
            var field = resolverContext.Field;

            if (objectType == null)
                throw new ArgumentNullException(nameof(resolverContext.ObjectType));

            if (field == null)
                throw new ArgumentNullException(nameof(resolverContext.Field));

            var fieldName = objectType.GetFieldName(field);

            if (string.IsNullOrEmpty(fieldName))
                throw new InvalidOperationException(
                    $"Object {objectType.Name} does not contain a field of type {field.Type.Name}");

            if (!TryGetValue(objectType.Name, out var objectNode))
            {
                return Task.FromResult((Resolver)null);
            }

            var resolver = objectNode.GetResolver(fieldName);
            return Task.FromResult(resolver);
        }

        public Task<Subscriber> GetSubscriberAsync(ResolverContext resolverContext)
        {
            var objectType = resolverContext.ObjectType;
            var field = resolverContext.Field;

            if (objectType == null)
                throw new ArgumentNullException(nameof(resolverContext.ObjectType));

            if (field == null)
                throw new ArgumentNullException(nameof(resolverContext.Field));

            var fieldName = objectType.GetFieldName(field);

            if (string.IsNullOrEmpty(fieldName))
                throw new InvalidOperationException(
                    $"Object {objectType.Name} does not contain a field of type {field.Type.Name}");

            if (!TryGetValue(objectType.Name, out var objectNode))
            {
                return Task.FromResult((Subscriber)null);
            }

            var resolver = objectNode.GetSubscriber(fieldName);
            return Task.FromResult(resolver);
        }
    }
}
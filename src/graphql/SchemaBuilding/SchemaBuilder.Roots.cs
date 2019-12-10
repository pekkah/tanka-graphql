using System.Collections.Generic;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class SchemaBuilder
    {
        public SchemaBuilder Query(
            out ObjectType query,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Query(_queryTypeName, out query, description, interfaces);
            return this;
        }

        public SchemaBuilder Query(
            string name,
            out ObjectType query,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object(name, out query, description, interfaces);
            _queryTypeName = name;
            return this;
        }

        public SchemaBuilder GetQuery(out ObjectType query)
        {
            if (!TryGetType(_queryTypeName, out query))
                throw new SchemaBuilderException(_queryTypeName, "Query root does not exists");

            return this;
        }

        public bool TryGetQuery(out ObjectType query)
        {
            return TryGetType(_queryTypeName, out query);
        }

        public SchemaBuilder Mutation(
            out ObjectType mutation,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Mutation(_mutationTypeName, out mutation, description, interfaces);
            return this;
        }

        public SchemaBuilder Mutation(
            string name,
            out ObjectType mutation,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object(name, out mutation, description, interfaces);
            _mutationTypeName = name;
            return this;
        }

        public SchemaBuilder GetMutation(out ObjectType mutation)
        {
            if (!TryGetType(_mutationTypeName, out mutation))
                throw new SchemaBuilderException(_mutationTypeName, "Mutation root does not exists");

            return this;
        }

        public SchemaBuilder Subscription(
            out ObjectType subscription,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Subscription(_subscriptionTypeName, out subscription, description, interfaces);
            return this;
        }

        public SchemaBuilder Subscription(
            string name,
            out ObjectType subscription,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object(name, out subscription, description, interfaces);
            _subscriptionTypeName = name;
            return this;
        }

        public SchemaBuilder GetSubscription(out ObjectType subscription)
        {
            if (!TryGetType(_subscriptionTypeName, out subscription))
                throw new SchemaBuilderException(_subscriptionTypeName, "Subscription root does not exists");

            return this;
        }
    }
}
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
            Query(QueryTypeName, out query, description, interfaces);
            return this;
        }

        public SchemaBuilder Query(
            string name,
            out ObjectType query,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object(name, out query, description, interfaces);
            QueryTypeName = name;
            return this;
        }

        public SchemaBuilder GetQuery(out ObjectType query)
        {
            if (!TryGetType(QueryTypeName, out query))
                throw new SchemaBuilderException(QueryTypeName, "Query root does not exists");

            return this;
        }

        public bool TryGetQuery(out ObjectType query)
        {
            return TryGetType(QueryTypeName, out query);
        }

        public SchemaBuilder Mutation(
            out ObjectType mutation,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Mutation(MutationTypeName, out mutation, description, interfaces);
            return this;
        }

        public SchemaBuilder Mutation(
            string name,
            out ObjectType mutation,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object(name, out mutation, description, interfaces);
            MutationTypeName = name;
            return this;
        }

        public SchemaBuilder GetMutation(out ObjectType mutation)
        {
            if (!TryGetType(MutationTypeName, out mutation))
                throw new SchemaBuilderException(MutationTypeName, "Mutation root does not exists");

            return this;
        }

        public bool TryGetMutation(out ObjectType query)
        {
            return TryGetType(MutationTypeName, out query);
        }

        public SchemaBuilder Subscription(
            out ObjectType subscription,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Subscription(SubscriptionTypeName, out subscription, description, interfaces);
            return this;
        }

        public SchemaBuilder Subscription(
            string name,
            out ObjectType subscription,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object(name, out subscription, description, interfaces);
            SubscriptionTypeName = name;
            return this;
        }

        public SchemaBuilder GetSubscription(out ObjectType subscription)
        {
            if (!TryGetType(SubscriptionTypeName, out subscription))
                throw new SchemaBuilderException(SubscriptionTypeName, "Subscription root does not exists");

            return this;
        }

        public bool TryGetSubscription(out ObjectType query)
        {
            return TryGetType(SubscriptionTypeName, out query);
        }
    }
}
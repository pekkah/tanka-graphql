using System.Collections.Generic;
using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public partial class SchemaBuilder
    {
        public SchemaBuilder Query(
            out ObjectType query,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object("Query", out query, description, interfaces);
            return this;
        }

        public SchemaBuilder Mutation(
            out ObjectType mutation,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object("Mutation", out mutation, description, interfaces);
            return this;
        }

        public SchemaBuilder Subscription(
            out ObjectType subscription,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object("Subscription", out subscription, description, interfaces);
            return this;
        }
    }
}
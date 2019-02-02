using tanka.graphql.introspection;
using tanka.graphql.type;
using Xunit;

// ReSharper disable InconsistentNaming

namespace tanka.graphql.tests.introspection
{
    public class IntrospectSchemaFacts
    {
        [Fact]
        public void Schema_Directives()
        {
            /* Given */
            var _ = new ObjectType(
                "Query",
                new Fields());

            var directive = new DirectiveType(
                "Directive",
                new[]
                {
                    DirectiveLocation.FIELD_DEFINITION
                },
                new Args
                {
                    {"arg1", ScalarType.Boolean}
                });

            var schema = new Schema(_, directives: new[] {directive});

            /* When */
            var __schema = Introspect.Schema(schema);

            /* Then */
            Assert.Single(__schema.DirectiveNames, directive.Name);
        }

        [Fact]
        public void Schema_MutationType()
        {
            /* Given */
            var _ = new ObjectType(
                "Query",
                new Fields());

            var mutation = new ObjectType(
                "Mutation",
                new Fields());

            var schema = new Schema(_, mutation);

            /* When */
            var __schema = Introspect.Schema(schema);

            /* Then */
            Assert.Equal(mutation.Name, __schema.MutationTypeName);
        }

        [Fact]
        public void Schema_QueryType()
        {
            /* Given */
            var query = new ObjectType(
                "Query",
                new Fields());

            var schema = new Schema(query);

            /* When */
            var __schema = Introspect.Schema(schema);

            /* Then */
            Assert.Equal(query.Name, __schema.QueryTypeName);
        }

        [Fact]
        public void Schema_SubscriptionType()
        {
            /* Given */
            var _ = new ObjectType(
                "Query",
                new Fields());

            var __ = new ObjectType(
                "Mutation",
                new Fields());

            var subscription = new ObjectType(
                "Subscription",
                new Fields());

            var schema = new Schema(_, __, subscription);

            /* When */
            var __schema = Introspect.Schema(schema);

            /* Then */
            Assert.Equal(subscription.Name, __schema.SubscriptionTypeName);
        }
    }
}
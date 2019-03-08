using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.resolvers
{
    public class ResolverBuilderFacts
    {
        [Fact]
        public async Task Should_chain_in_order1()
        {
            /* Given */
            var values = new List<int>();
            var builder = new ResolverBuilder();
            builder.Use((context, next) =>
            {
                values.Add(0);
                return next(context);
            });

            builder.Use((context, next) =>
            {
                values.Add(1);
                return next(context);
            });

            builder.Use((context, next) =>
            {
                values.Add(2);
                return next(context);
            });

            builder.Use(context => new ValueTask<IResolveResult>(Resolve.As(42)));

            /* When */
            var resolver = builder.Build();
            await resolver(null);

            /* Then */
            Assert.Equal(new[] {0, 1, 2}, values.ToArray());
        }

        [Fact]
        public async Task Should_chain_in_order2()
        {
            /* Given */
            var values = new List<int>();
            var builder = new ResolverBuilder();
            builder.Use((context, next) =>
            {
                values.Add(0);
                return next(context);
            });

            builder.Use((context, next) =>
            {
                var result = next(context);
                values.Add(1);
                return result;
            });

            builder.Use((context, next) =>
            {
                values.Add(2);
                return next(context);
            });

            builder.Use(context => new ValueTask<IResolveResult>(Resolve.As(42)));

            /* When */
            var resolver = builder.Build();
            await resolver(null);

            /* Then */
            Assert.Equal(new[] {0, 2, 1}, values.ToArray());
        }

        [Fact]
        public async Task Should_propagate_resolved_value()
        {
            /* Given */
            var builder = new ResolverBuilder();
            builder
                .Use((context, next) => next(context))
                .Use((context, next) => next(context))
                .Use((context, next) => next(context));

            builder.Use(context => new ValueTask<IResolveResult>(Resolve.As(42)));

            /* When */
            var resolver = builder.Build();
            var result = await resolver(null);

            /* Then */
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void Should_not_call_chain_until_resolver_executed()
        {
            /* Given */
            var values = new List<int>();
            var builder = new ResolverBuilder();
            builder.Use((context, next) =>
            {
                values.Add(0);
                return next(context);
            });

            builder.Use(context => new ValueTask<IResolveResult>(Resolve.As(42)));

            /* When */
            builder.Build();

            /* Then */
            Assert.Equal(new int[] {}, values.ToArray());
        }
    }
}
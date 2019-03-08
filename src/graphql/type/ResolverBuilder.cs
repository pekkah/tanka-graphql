using System.Collections.Generic;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public class ResolverBuilder
    {
        private readonly List<ResolverMiddleware> _middlewares
            = new List<ResolverMiddleware>();

        public ResolverBuilder Use(ResolverMiddleware middleware)
        {
            _middlewares.Insert(0, middleware);
            return this;
        }

        public ResolverBuilder Use(Resolver resolver)
        {
            _middlewares.Insert(0, (context, next) => resolver(context));

            return this;
        }

        public Resolver Build()
        {
            Resolver resolver = null;
            foreach (var middleware in _middlewares)
            {
                var resolver1 = resolver;
                resolver = context => middleware(context, resolver1);
            }

            return resolver;
        }
    }
}
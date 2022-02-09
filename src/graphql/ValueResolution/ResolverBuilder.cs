using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.ValueResolution
{
    public class ResolverBuilder
    {
        private readonly List<ResolverMiddleware> _middlewares = new();

        private Resolver? _root;

        public ResolverBuilder(Resolver root)
        {
            Run(root);
        }

        public ResolverBuilder()
        {
        }

        /// <summary>
        ///     Add middleware to be run before the root resolver
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public ResolverBuilder Use(ResolverMiddleware middleware)
        {
            _middlewares.Insert(0, middleware);
            return this;
        }

        /// <summary>
        ///     Set root resolver to be run at the end of the resolver chain
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public ResolverBuilder Run(Resolver resolver)
        {
            _root = resolver;
            return this;
        }

        public Resolver Build()
        {
            if (_root is null)
                throw new InvalidOperationException($"Resolver chain is missing ending. Use {nameof(Run)} to end the chain.");

            var resolver = _root;
            foreach (var middleware in _middlewares)
            {
                var resolver1 = resolver;
                resolver = context => middleware(context, resolver1);
            }

            return resolver;
        }
    }
}
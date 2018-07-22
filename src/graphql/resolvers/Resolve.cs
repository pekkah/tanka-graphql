using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.type;

namespace fugu.graphql.resolvers
{
    public static class Resolve
    {
        public static IResolveResult As(object result)
        {
            return new ResolveResult(result);
        }

        public static IResolveResult As(ObjectType type, object result)
        {
            return new ResolveResult(type, result);
        }

        public static IResolveResult As(IEnumerable result)
        {
            return new ResolveResult(result);
        }

        public static Resolver PropertyOf<T>(Func<T, object> getValue)
        {
            return context =>
            {
                var source = (T) context.ObjectValue;
                var value = getValue(source);
                return Task.FromResult(As(value));
            };
        }

        public static Resolver PropertyOf<T>(Func<T, IEnumerable<object>> getValue)
        {
            return context =>
            {
                var source = (T) context.ObjectValue;
                var values = getValue(source);

                if (values == null)
                    return Task.FromResult(As(null));

                return Task.FromResult(As(values.Select(As)));
            };
        }

        public static ISubscribeResult Stream(ISourceBlock<object> reader, Func<Task> unsubscribeAsync)
        {
            return new SubscribeResult(reader, unsubscribeAsync);
        }
    }
}
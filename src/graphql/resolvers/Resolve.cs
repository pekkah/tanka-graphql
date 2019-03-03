using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using tanka.graphql.type;

namespace tanka.graphql.resolvers
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
                var source = context.ObjectValue is T objectValue ? objectValue : default(T);

                if (source == null)
                {
                    return new ValueTask<IResolveResult>(As(null));
                }

                var value = getValue(source);
                return new ValueTask<IResolveResult>(As(value));
            };
        }

        public static Resolver PropertyOf<T>(Func<T, IEnumerable<object>> getValue)
        {
            return context =>
            {
                var source = context.ObjectValue is T objectValue ? objectValue : default(T);

                if (source == null)
                {
                    return new ValueTask<IResolveResult>(As(null));
                }

                var values = getValue(source);

                if (values == null)
                    return new ValueTask<IResolveResult>(As(null));

                return new ValueTask<IResolveResult>(As(values.Select(As)));
            };
        }

        public static ISubscribeResult Stream(ISourceBlock<object> reader)
        {
            return new SubscribeResult(reader);
        }
    }
}
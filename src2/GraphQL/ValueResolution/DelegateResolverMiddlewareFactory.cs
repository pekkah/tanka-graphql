using System.Linq.Expressions;
using System.Reflection;

namespace Tanka.GraphQL.ValueResolution;

public class DelegateResolverMiddlewareFactory : DelegateFactoryBase<ResolverContext, Func<ResolverContext, Resolver, ValueTask>>
{
    private static readonly Lazy<DelegateResolverMiddlewareFactory> InstanceFactory = new(() => new DelegateResolverMiddlewareFactory());

    public static DelegateResolverMiddlewareFactory Instance => InstanceFactory.Value;

    private static readonly ParameterExpression NextParam = Expression.Parameter(typeof(Resolver), "next");

    public static Func<ResolverContext, Resolver, ValueTask> Get(Delegate middlewareDelegate)
    {
        return Instance.GetOrCreate(middlewareDelegate);
    }

    protected override Expression GetArgumentExpression(ParameterInfo p, IReadOnlyDictionary<string, Expression> contextProperties)
    {
        if (p.ParameterType == typeof(Resolver))
            return NextParam;

        return base.GetArgumentExpression(p, contextProperties);
    }

    public override Func<ResolverContext, Resolver, ValueTask> Create(Delegate resolverDelegate)
    {
        MethodInfo invokeMethod = resolverDelegate.Method;

        MethodCallExpression invokeExpression = GetDelegateMethodCallExpression(
            invokeMethod,
            resolverDelegate.Target
        );

        Expression valueTaskExpression;
        if (invokeMethod.ReturnType == typeof(ValueTask))
        {
            valueTaskExpression = invokeExpression;
        }
        else if (invokeMethod.ReturnType == typeof(Task))
        {
            valueTaskExpression = Expression.New(
                typeof(ValueTask).GetConstructor(new[] { typeof(Task) })!,
                invokeExpression
            );
        }
        else if (invokeMethod.ReturnType == typeof(void))
        {
            valueTaskExpression = Expression.Block(
                invokeExpression,
                Expression.Constant(ValueTask.CompletedTask)
            );
        }
        else if (invokeMethod.ReturnType.IsGenericType &&
                 invokeMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            Type t = invokeMethod.ReturnType.GetGenericArguments()[0];
            valueTaskExpression =
                Expression.Call(DelegateResolverFactory.ResolveValueTaskMethod.MakeGenericMethod(t), invokeExpression, ContextParam);
        }
        else if (invokeMethod.ReturnType.IsGenericType &&
                 invokeMethod.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            Type t = invokeMethod.ReturnType.GetGenericArguments()[0];
            valueTaskExpression = Expression.Call(DelegateResolverFactory.ResolveValueValueTaskMethod.MakeGenericMethod(t), invokeExpression,
                ContextParam);
        }
        else
        {
            Type t = invokeMethod.ReturnType;
            valueTaskExpression = Expression.Call(DelegateResolverFactory.ResolveValueObjectMethod.MakeGenericMethod(t), invokeExpression,
                ContextParam);
        }


        var lambda = Expression.Lambda<Func<ResolverContext, Resolver, ValueTask>>(
            valueTaskExpression,
            ContextParam,
            NextParam
        );

        var compiledLambda = lambda.Compile();
        return compiledLambda;
    }
}
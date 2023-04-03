using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Tanka.GraphQL.ValueResolution;

public class DelegateSubscriberMiddlewareFactory : DelegateFactoryBase<SubscriberContext, Func<SubscriberContext, Subscriber, CancellationToken, ValueTask>>
{
    private static readonly Lazy<DelegateSubscriberMiddlewareFactory> InstanceFactory = new(() => new DelegateSubscriberMiddlewareFactory());

    public static DelegateSubscriberMiddlewareFactory Instance => InstanceFactory.Value;

    private static readonly ParameterExpression NextParam = Expression.Parameter(typeof(Subscriber), "next");

    public readonly ParameterExpression CancellationTokenParam =
        Expression.Parameter(typeof(CancellationToken), "unsubscribe");

    public static Func<SubscriberContext, Subscriber, CancellationToken, ValueTask> Get(Delegate middlewareDelegate)
    {
        return Instance.GetOrCreate(middlewareDelegate);
    }

    protected override Expression GetArgumentExpression(ParameterInfo p, IReadOnlyDictionary<string, Expression> contextProperties)
    {
        if (p.ParameterType == typeof(Subscriber))
            return NextParam;

        if (p.ParameterType == typeof(CancellationToken))
            return CancellationTokenParam;

        return base.GetArgumentExpression(p, contextProperties);
    }

    public override Func<SubscriberContext, Subscriber, CancellationToken, ValueTask> Create(Delegate subscriberDelegate)
    {
        MethodInfo invokeMethod = subscriberDelegate.Method;
        MethodCallExpression invokeExpression = GetDelegateMethodCallExpression(
            invokeMethod,
            subscriberDelegate.Target
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
                 invokeMethod.ReturnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
        {
            Type t = invokeMethod.ReturnType.GetGenericArguments()[0];
            valueTaskExpression = Expression.Call(DelegateSubscriberFactory.ResolveAsyncEnumerableT.MakeGenericMethod(t), invokeExpression,
                CancellationTokenParam, ContextParam);
        }
        else
        {
            throw new InvalidAsynchronousStateException(
                "Subscriber delegate return value must be of type IAsyncEnumerable<T>.");
        }


        var lambda = Expression.Lambda<Func<SubscriberContext, Subscriber, CancellationToken, ValueTask>>(
            valueTaskExpression,
            ContextParam,
            NextParam,
            CancellationTokenParam
        );

        var compiledLambda = lambda.Compile();
        return compiledLambda;
    }
}
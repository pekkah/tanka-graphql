using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Tanka.GraphQL.ValueResolution;

public class DelegateSubscriberFactory : DelegateFactoryBase<SubscriberContext, Subscriber>
{
    internal static readonly MethodInfo ResolveAsyncEnumerableT = typeof(DelegateSubscriberFactory)
        .GetMethod(nameof(ResolveAsyncEnumerable), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly Lazy<DelegateSubscriberFactory>
        InstanceFactory = new(() => new DelegateSubscriberFactory());

    public readonly ParameterExpression CancellationTokenParam =
        Expression.Parameter(typeof(CancellationToken), "unsubscribe");

    public static DelegateSubscriberFactory Instance => InstanceFactory.Value;

    public static Subscriber Get(Delegate subscriberDelegate)
    {
        return Instance.GetOrCreate(subscriberDelegate);
    }

    protected override Expression GetArgumentExpression(ParameterInfo p, IReadOnlyDictionary<string, Expression> contextProperties)
    {
        if (p.ParameterType == typeof(CancellationToken))
            return CancellationTokenParam;

        return base.GetArgumentExpression(p, contextProperties);

    }

    public override Subscriber Create(Delegate subscriberDelegate)
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
            valueTaskExpression = Expression.Call(ResolveAsyncEnumerableT.MakeGenericMethod(t), invokeExpression,
                CancellationTokenParam, ContextParam);
        }
        else
        {
            throw new InvalidAsynchronousStateException(
                "Subscriber delegate return value must be of type IAsyncEnumerable<T>.");
        }


        var lambda = Expression.Lambda<Subscriber>(
            valueTaskExpression,
            ContextParam,
            CancellationTokenParam
        );

        Subscriber compiledLambda = lambda.Compile();
        return compiledLambda;
    }

    private static ValueTask ResolveAsyncEnumerable<T>(IAsyncEnumerable<T> task, CancellationToken cancellationToken,
        SubscriberContext context)
    {
        context.ResolvedValue = Wrap(task, cancellationToken);
        return ValueTask.CompletedTask;

        [DebuggerStepThrough]
        static async IAsyncEnumerable<object?> Wrap(
            IAsyncEnumerable<T> task,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (T item in task.WithCancellation(cancellationToken)) yield return item;
        }
    }
}
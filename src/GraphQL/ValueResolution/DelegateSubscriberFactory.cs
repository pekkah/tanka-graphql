using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.ValueResolution;

public static class DelegateSubscriberFactory
{
    private static readonly ParameterExpression ContextParam = Expression.Parameter(typeof(SubscriberContext), "context");
    private static readonly ParameterExpression CancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

    private static readonly IReadOnlyDictionary<string, Expression> ContextParamProperties = typeof(SubscriberContext)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .ToDictionary(p => p.Name.ToLowerInvariant(), p => (Expression)Expression.Property(ContextParam, p));


    private static readonly ConcurrentDictionary<Delegate, Subscriber> Cache = new();

    public static Subscriber GetOrCreate(Delegate subscriberDelegate)
    {
        if (Cache.TryGetValue(subscriberDelegate, out var resolver))
            return resolver;

        return Create(subscriberDelegate);
    }

    public static Subscriber Create(Delegate subscriberDelegate)
    {
#if DEBUG
        Trace.WriteLine(
            $"Available context parameters:\n {string.Join(',', ContextParamProperties.Select(p => string.Concat($"{p.Key}: {p.Value.Type}")))}");
#endif

        MethodInfo invokeMethod = subscriberDelegate.Method;

        Expression instanceExpression = null;
        if (!invokeMethod.IsStatic) instanceExpression = Expression.Constant(subscriberDelegate.Target);

        IEnumerable<Expression> argumentsExpressions = invokeMethod.GetParameters()
            .Select(p =>
            {
                if (p.ParameterType == typeof(SubscriberContext)) return ContextParam;
                if (p.ParameterType == typeof(CancellationToken)) return CancellationTokenParam;

                if (p.Name is not null)
                    if (ContextParamProperties.TryGetValue(p.Name.ToLowerInvariant(),
                            out Expression? propertyExpression))
                    {
                        if (p.ParameterType == propertyExpression.Type)
                            return propertyExpression;

                        return Expression.Convert(propertyExpression, p.ParameterType);
                    }

                MemberExpression serviceProviderProperty = Expression.Property(ContextParam, "RequestServices");
                MethodInfo? getServiceMethodInfo = typeof(ServiceProviderServiceExtensions)
                    .GetMethods()
                    .FirstOrDefault(m => m.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService)
                                         && m.GetParameters().Length == 1
                                         && m.GetParameters()[0].ParameterType == typeof(IServiceProvider)
                                         && m.IsGenericMethodDefinition);

                if (getServiceMethodInfo is null)
                    throw new InvalidOperationException("Could not find GetRequiredService method");

                Type serviceType = p.ParameterType;
                MethodInfo genericMethodInfo = getServiceMethodInfo.MakeGenericMethod(serviceType);
                var getRequiredServiceCall = (Expression)Expression.Call(
                    null,
                    genericMethodInfo,
                    serviceProviderProperty);

                return Expression.Block(
                    getRequiredServiceCall
                );
            });

        MethodCallExpression invokeExpression = Expression.Call(
            instanceExpression,
            invokeMethod,
            argumentsExpressions
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
            var t = invokeMethod.ReturnType.GetGenericArguments()[0];
            valueTaskExpression = Expression.Call(ResolveAsyncEnumerableT.MakeGenericMethod(t), invokeExpression, CancellationTokenParam,ContextParam);
        }
        else
        {
            throw new InvalidAsynchronousStateException($"Subscriber delegate return value must be of type IAsyncEnumerable<T>.");
        }


        var lambda = Expression.Lambda<Subscriber>(
            valueTaskExpression,
            ContextParam,
            CancellationTokenParam
        );

        var compiledLambda = lambda.Compile();
        Cache.TryAdd(subscriberDelegate, compiledLambda);
        return compiledLambda;
    }

    private static readonly MethodInfo ResolveAsyncEnumerableT = typeof(DelegateSubscriberFactory)
        .GetMethod(nameof(ResolveAsyncEnumerable), BindingFlags.Static | BindingFlags.NonPublic)!;


    private static ValueTask ResolveAsyncEnumerable<T>(IAsyncEnumerable<T> task, CancellationToken cancellationToken, SubscriberContext context)
    {

        context.ResolvedValue = Wrap(task, cancellationToken);
        return ValueTask.CompletedTask;

        [DebuggerStepThrough]
        static async IAsyncEnumerable<object?> Wrap(
            IAsyncEnumerable<T> task,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in task.WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }
    }

}
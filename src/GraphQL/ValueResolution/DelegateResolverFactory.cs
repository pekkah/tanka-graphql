using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.ValueResolution;

public static class DelegateResolverFactory
{
    private static readonly ParameterExpression ContextParam = Expression.Parameter(typeof(ResolverContext), "context");

    private static readonly IReadOnlyDictionary<string, Expression> ContextParamProperties = typeof(ResolverContext)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .ToDictionary(p => p.Name.ToLowerInvariant(), p => (Expression)Expression.Property(ContextParam, p));


    private static readonly ConcurrentDictionary<Delegate, Resolver> Cache = new();


    private static readonly MethodInfo ThrowMethod = typeof(DelegateResolverFactory)
        .GetMethod(nameof(Throw), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo GetArgumentMethod = typeof(ArgumentsResolverContextExtensions)
        .GetMethod(
            nameof(ArgumentsResolverContextExtensions.GetArgument),
            new[] { typeof(ResolverContextBase), typeof(string) }
        )!;

    private static readonly MethodInfo BindInputObjectMethod = typeof(ArgumentsResolverContextExtensions)
        .GetMethod(
            nameof(ArgumentsResolverContextExtensions.BindInputObject),
            new[] { typeof(ResolverContextBase), typeof(string) }
        )!;

    private static readonly MethodInfo BindInputObjectListMethod = typeof(ArgumentsResolverContextExtensions)
        .GetMethod(
            nameof(ArgumentsResolverContextExtensions.BindInputObjectList),
            new[] { typeof(ResolverContextBase), typeof(string) }
        )!;

    private static readonly MethodInfo HasArgumentMethod = typeof(ArgumentsResolverContextExtensions)
        .GetMethod(
            nameof(ArgumentsResolverContextExtensions.HasArgument),
            new[] { typeof(ResolverContextBase), typeof(string) }
        )!;

    private static readonly MethodInfo ResolveValueTaskMethod = typeof(DelegateResolverFactory)
        .GetMethod(nameof(ResolveValueTask), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo ResolveValueValueTaskMethod = typeof(DelegateResolverFactory)
        .GetMethod(nameof(ResolveValueValueTask), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo ResolveValueObjectMethod = typeof(DelegateResolverFactory)
        .GetMethod(nameof(ResolveValueObject), BindingFlags.Static | BindingFlags.NonPublic)!;

    public static Resolver Create(Delegate resolverDelegate)
    {
#if DEBUG
        Trace.WriteLine(
            $"Available context parameters:\n {string.Join(',', ContextParamProperties.Select(p => string.Concat($"{p.Key}: {p.Value.Type}")))}");
#endif

        MethodInfo invokeMethod = resolverDelegate.Method;

        Expression instanceExpression = null;
        if (!invokeMethod.IsStatic) instanceExpression = Expression.Constant(resolverDelegate.Target);

        IEnumerable<Expression> argumentsExpressions = invokeMethod.GetParameters()
            .Select(p =>
            {
                if (p.ParameterType == typeof(ResolverContext)) return ContextParam;

                if (p.Name is not null)
                    if (ContextParamProperties.TryGetValue(p.Name.ToLowerInvariant(),
                            out Expression? propertyExpression))
                    {
                        if (p.ParameterType == propertyExpression.Type)
                            return propertyExpression;

                        return Expression.Convert(propertyExpression, p.ParameterType);
                    }

                Expression hasArgumentCall = HasArgumentCall(p);
                Expression resolveArgumentCall = ResolveArgumentCall(p);
                Expression getRequiredServiceCall = GetRequiredServiceCall(p);
                return Expression.Condition(
                    hasArgumentCall,
                    resolveArgumentCall,
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
                 invokeMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            Type t = invokeMethod.ReturnType.GetGenericArguments()[0];
            valueTaskExpression =
                Expression.Call(ResolveValueTaskMethod.MakeGenericMethod(t), invokeExpression, ContextParam);
        }
        else if (invokeMethod.ReturnType.IsGenericType &&
                 invokeMethod.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            Type t = invokeMethod.ReturnType.GetGenericArguments()[0];
            valueTaskExpression = Expression.Call(ResolveValueValueTaskMethod.MakeGenericMethod(t), invokeExpression,
                ContextParam);
        }
        else
        {
            Type t = invokeMethod.ReturnType;
            valueTaskExpression = Expression.Call(ResolveValueObjectMethod.MakeGenericMethod(t), invokeExpression,
                ContextParam);
        }


        var lambda = Expression.Lambda<Resolver>(
            valueTaskExpression,
            ContextParam
        );

        Resolver compiledLambda = lambda.Compile();
        Cache.TryAdd(resolverDelegate, compiledLambda);
        return compiledLambda;
    }

    public static Resolver GetOrCreate(Delegate resolverDelegate)
    {
        if (Cache.TryGetValue(resolverDelegate, out Resolver? resolver))
            return resolver;

        return Create(resolverDelegate);
    }

    private static Expression GetRequiredServiceCall(ParameterInfo p)
    {
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
        return getRequiredServiceCall;
    }


    private static Expression HasArgumentCall(ParameterInfo p)
    {
        ArgumentException.ThrowIfNullOrEmpty(p.Name);
        ConstantExpression name = Expression.Constant(p.Name, typeof(string));
        return Expression.Call(
            null,
            HasArgumentMethod,
            ContextParam,
            name);
    }

    private static bool IsPrimitiveOrNullablePrimitive(Type type)
    {
        if (type.IsPrimitive)
            return true;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return type.GetGenericArguments()[0].IsPrimitive;
        return false;
    }

    private static Expression ResolveArgumentCall(ParameterInfo p)
    {
        ConstantExpression nameParam = Expression.Constant(p.Name, typeof(string));

        Expression? callExpr;
        if (IsPrimitiveOrNullablePrimitive(p.ParameterType))
        {
            MethodInfo methodInfo = GetArgumentMethod.MakeGenericMethod(p.ParameterType);
            callExpr = Expression.Call(methodInfo, ContextParam, nameParam);
        }
        else if (p.ParameterType == typeof(string))
        {
            MethodInfo methodInfo = GetArgumentMethod.MakeGenericMethod(p.ParameterType);
            callExpr = Expression.Call(methodInfo, ContextParam, nameParam);
        }
        else if (p.ParameterType.IsGenericType
                 && p.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                 && (p.ParameterType.GetGenericArguments()[0].IsClass ||
                     (p.ParameterType.GetGenericArguments()[0].IsValueType &&
                      p.ParameterType.GetGenericArguments()[0].GetConstructor(Type.EmptyTypes) != null)))
        {
            MethodInfo methodInfo =
                BindInputObjectListMethod.MakeGenericMethod(p.ParameterType.GetGenericArguments()[0]);
            callExpr = Expression.Call(methodInfo, ContextParam, nameParam);
        }
        else if (p.ParameterType.IsClass)
        {
            MethodInfo methodInfo = BindInputObjectMethod.MakeGenericMethod(p.ParameterType);
            callExpr = Expression.Call(methodInfo, ContextParam, nameParam);
        }
        else
        {
            MethodInfo methodInfo = ThrowMethod.MakeGenericMethod(p.ParameterType);
            callExpr = Expression.Call(methodInfo,
                Expression.Constant($"Unsupported parameter type '{p.ParameterType}' " +
                                    $"for parameter '{p.Name}'. Only primitive types, classes " +
                                    "and structs with a default constructor are supported."));
        }


        //callExpr = Expression.Convert(callExpr, p.ParameterType);

        return callExpr;
    }

    private static ValueTask ResolveValueObject<T>(T result, ResolverContext context)
    {
        context.ResolvedValue = result;
        return ValueTask.CompletedTask;
    }

    private static ValueTask ResolveValueTask<T>(Task<T> task, ResolverContext context)
    {
        static async ValueTask AwaitResolveValue(Task<T> task, ResolverContext context)
        {
            context.ResolvedValue = await task;
        }

        if (task.IsCompletedSuccessfully)
        {
            context.ResolvedValue = task.Result;
            return default(ValueTask);
        }

        return AwaitResolveValue(task, context);
    }

    private static ValueTask ResolveValueValueTask<T>(ValueTask<T> task, ResolverContext context)
    {
        static async ValueTask AwaitResolveValue(ValueTask<T> task, ResolverContext context)
        {
            context.ResolvedValue = await task;
        }

        if (task.IsCompletedSuccessfully)
        {
            context.ResolvedValue = task.Result;
            return default(ValueTask);
        }

        return AwaitResolveValue(task, context);
    }

    private static T? Throw<T>(string message)
    {
        throw new ArgumentException(message);
    }
}
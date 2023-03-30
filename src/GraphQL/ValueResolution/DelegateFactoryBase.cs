using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.ValueResolution;

public abstract class DelegateFactoryBase<TContext, TLambda>
    where TContext : ResolverContextBase
{
    private readonly ConcurrentDictionary<Delegate, TLambda> _cache = new();

    protected readonly MethodInfo BindInputObjectListMethod = typeof(ArgumentsResolverContextExtensions)
        .GetMethod(
            nameof(ArgumentsResolverContextExtensions.BindInputObjectList),
            new[] { typeof(ResolverContextBase), typeof(string) }
        )!;

    protected readonly MethodInfo BindInputObjectMethod = typeof(ArgumentsResolverContextExtensions)
        .GetMethod(
            nameof(ArgumentsResolverContextExtensions.BindInputObject),
            new[] { typeof(ResolverContextBase), typeof(string) }
        )!;

    public readonly ParameterExpression ContextParam =
        Expression.Parameter(typeof(TContext), "context");


    protected readonly MethodInfo HasArgumentMethod = typeof(ArgumentsResolverContextExtensions)
        .GetMethod(
            nameof(ArgumentsResolverContextExtensions.HasArgument),
            new[] { typeof(ResolverContextBase), typeof(string) }
        )!;

    protected readonly MethodInfo ThrowMethod = typeof(DelegateFactoryBase<TContext, TLambda>)
        .GetMethod(nameof(Throw), BindingFlags.NonPublic | BindingFlags.Static)!;

    protected MethodInfo GetArgumentMethod = typeof(ArgumentsResolverContextExtensions)
        .GetMethod(
            nameof(ArgumentsResolverContextExtensions.GetArgument),
            new[] { typeof(ResolverContextBase), typeof(string) }
        )!;

    public abstract TLambda Create(Delegate subscriberDelegate);

    public TLambda GetOrCreate(Delegate delegateFunc)
    {
        if (_cache.TryGetValue(delegateFunc, out TLambda? resolver))
            return resolver;

        resolver = Create(delegateFunc);
        _cache.TryAdd(delegateFunc, resolver);
        return resolver;
    }

    protected virtual IEnumerable<Expression> GetArgumentExpressions(MethodInfo invokeMethod)
    {
        IReadOnlyDictionary<string, Expression> contextProperties = GetContextParamProperties();
        return invokeMethod.GetParameters()
            .Select(p => GetArgumentExpression(p, contextProperties));
    }

    protected virtual Expression GetArgumentExpression(ParameterInfo p, IReadOnlyDictionary<string, Expression> contextProperties)
    {
        if (p.ParameterType == typeof(TContext))
            return ContextParam;

        if (p.Name is not null)
            if (contextProperties.TryGetValue(p.Name.ToLowerInvariant(),
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
    }


    protected IReadOnlyDictionary<string, Expression> GetContextParamProperties()
    {
        return typeof(TContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name.ToLowerInvariant(), p => (Expression)Expression.Property(ContextParam, p));
    }

    protected MethodCallExpression GetDelegateMethodCallExpression(MethodInfo method, object? target)
    {
        Expression? instanceExpression = null;

        if (!method.IsStatic) instanceExpression = Expression.Constant(target);

        IEnumerable<Expression> argumentsExpressions = GetArgumentExpressions(method);
        MethodCallExpression invokeExpression = Expression.Call(
            instanceExpression,
            method,
            argumentsExpressions
        );

        return invokeExpression;
    }

    private Expression GetRequiredServiceCall(ParameterInfo p)
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


    private Expression HasArgumentCall(ParameterInfo p)
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

    private Expression ResolveArgumentCall(ParameterInfo p)
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

    private static T? Throw<T>(string message)
    {
        throw new ArgumentException(message);
    }
}
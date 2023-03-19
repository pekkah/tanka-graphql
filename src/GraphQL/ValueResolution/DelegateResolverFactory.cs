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

    public static Resolver Create(Delegate resolverDelegate)
    {
#if DEBUG
        Trace.WriteLine($"Available context parameters:\n {string.Join(',', ContextParamProperties.Select(p => string.Concat($"{p.Key}: {p.Value.Type}")))}");
#endif

        var invokeMethod = resolverDelegate.Method;

        Expression instanceExpression = null;
        if (!invokeMethod.IsStatic)
        {
            instanceExpression = Expression.Constant(resolverDelegate.Target);
        }

        IEnumerable<Expression> argumentsExpressions = invokeMethod.GetParameters()
            .Select(p =>
            {
                if (p.ParameterType == typeof(ResolverContext))
                {
                    return ContextParam;
                }

                if (p.Name is not null)
                    if (ContextParamProperties.TryGetValue(p.Name.ToLowerInvariant(), out var propertyExpression))
                    {
                        if (p.ParameterType == propertyExpression.Type)
                            return propertyExpression;

                        return Expression.Convert(propertyExpression, p.ParameterType);
                    }

                var serviceProviderProperty = Expression.Property(ContextParam, "RequestServices");
                var getServiceMethodInfo = typeof(ServiceProviderServiceExtensions)
                    .GetMethods()
                    .FirstOrDefault(m => m.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService)
                                         && m.GetParameters().Length == 1
                                         && m.GetParameters()[0].ParameterType == typeof(IServiceProvider)
                                         && m.IsGenericMethodDefinition);

                if (getServiceMethodInfo is null)
                    throw new InvalidOperationException("Could not find GetRequiredService method");

                var serviceType = p.ParameterType;
                var genericMethodInfo = getServiceMethodInfo.MakeGenericMethod(serviceType);
                var getRequiredServiceCall = (Expression)Expression.Call(
                    null,
                    genericMethodInfo,
                    serviceProviderProperty);

                return (Expression)Expression.Block(
                    getRequiredServiceCall
                );
            });

        var invokeExpression = Expression.Call(
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
                typeof(ValueTask).GetConstructor(new[] { typeof(Task) }),
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
        else
        {
            throw new ArgumentException($"Unsupported delegate return type {invokeMethod.ReturnType}", nameof(resolverDelegate));
        }


        var lambda = Expression.Lambda<Resolver>(
            valueTaskExpression,
            ContextParam
        );

        return lambda.Compile();
    }
}
using System.Linq.Expressions;
using Tanka.GraphQL.ValueResolution;


public static class DelegateResolverFactory
{
    public static Resolver Create(Delegate resolverDelegate)
    {
        var contextParam = Expression.Parameter(typeof(ResolverContext), "context");
        var invokeMethod = resolverDelegate.Method;

        Expression instanceExpression = null;
        if (!invokeMethod.IsStatic)
        {
            instanceExpression = Expression.Constant(resolverDelegate.Target);
        }

        var invokeExpression = Expression.Call(
            instanceExpression,
            invokeMethod,
            contextParam
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
            contextParam
        );

        return lambda.Compile();
    }



}

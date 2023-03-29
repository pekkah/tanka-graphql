using System.Linq.Expressions;
using System.Reflection;

namespace Tanka.GraphQL.ValueResolution;

public class DelegateResolverFactory : DelegateFactoryBase<ResolverContext, Resolver>
{
    private static readonly MethodInfo ResolveValueTaskMethod = typeof(DelegateResolverFactory)
        .GetMethod(nameof(ResolveValueTask), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo ResolveValueValueTaskMethod = typeof(DelegateResolverFactory)
        .GetMethod(nameof(ResolveValueValueTask), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo ResolveValueObjectMethod = typeof(DelegateResolverFactory)
        .GetMethod(nameof(ResolveValueObject), BindingFlags.Static | BindingFlags.NonPublic)!;
    
    private static readonly Lazy<DelegateResolverFactory> InstanceFactory = new(() => new DelegateResolverFactory());

    public static DelegateResolverFactory Instance => InstanceFactory.Value;

    public static Resolver Get(Delegate resolverDelegate)
    {
        return Instance.GetOrCreate(resolverDelegate);
    }

    public override Resolver Create(Delegate resolverDelegate)
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
        return compiledLambda;
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
}
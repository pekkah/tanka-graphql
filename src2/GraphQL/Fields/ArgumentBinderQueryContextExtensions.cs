using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Fields;

public static class ArgumentBinderQueryContextExtensions
{
    public static T? BindInputObject<T>(this QueryContext queryContext, ResolverContextBase context, string name)
        where T : new()
    {
        ArgumentNullException.ThrowIfNull(queryContext.ArgumentBinder);

        return queryContext.ArgumentBinder.BindInputObject<T>(context, name);
    }

    public static IEnumerable<T?>? BindInputObjectList<T>(this QueryContext queryContext, ResolverContextBase context,
        string name)
        where T : new()
    {
        ArgumentNullException.ThrowIfNull(queryContext.ArgumentBinder);

        return queryContext.ArgumentBinder.BindInputObjectList<T>(context, name);
    }

    

    public static bool HasArgument(
        this QueryContext queryContext, 
        ResolverContextBase context,
        string name)
    {
        ArgumentNullException.ThrowIfNull(queryContext.ArgumentBinder);

        return queryContext.ArgumentBinder.HasArgument(context, name);
    }
}
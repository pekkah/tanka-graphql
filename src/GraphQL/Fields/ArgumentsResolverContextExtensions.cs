using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Fields;

public static class ArgumentsResolverContextExtensions
{
    public static T? GetArgument<T>(this ResolverContextBase context, string name)
    {
        if (!context.Arguments.TryGetValue(name, out var arg))
            throw new ArgumentOutOfRangeException(nameof(name), name,
                $"Field '{context.Field.Name}' does not contain argument with name '{name}''");

        return (T?)arg;
    }

    public static T? BindInputObject<T>(this ResolverContextBase context, string name)
        where T : new()
    {
        return context.QueryContext.BindInputObject<T>(context, name);
    }
    public static IEnumerable<T?>? BindInputObjectList<T>(this ResolverContextBase context, string name)
        where T : new()
    {
        return context.QueryContext.BindInputObjectList<T>(context, name);
    }
}
using System.Reflection;
using Tanka.GraphQL.Internal;

namespace Tanka.GraphQL.Fields;

public static class ArgumentBinderQueryContextExtensions
{
    public static T? BindInputObject<T>(this QueryContext queryContext, ResolverContext context, string name)
        where T : new()
    {
        return queryContext.ArgumentBinderFeature.BindInputObject<T>(context, name);
    }

    public static IEnumerable<T?>? BindInputObjectList<T>(this QueryContext queryContext, ResolverContext context,
        string name)
        where T : new()
    {
        return queryContext.ArgumentBinderFeature.BindInputObjectList<T>(context, name);
    }
}

public interface IArgumentBinderFeature
{
    static IArgumentBinderFeature Default = new ArgumentBinderFeature();

    T? BindInputObject<T>(ResolverContext context, string name)
        where T : new();

    IEnumerable<T?>? BindInputObjectList<T>(ResolverContext context, string name) where T : new();
}

public class ArgumentBinderFeature : IArgumentBinderFeature
{
    private static Dictionary<Type, List<PropertyInfo>> _cache = new();

    public T? BindInputObject<T>(ResolverContext context, string name)
        where T : new()
    {
        var argument = context.Arguments[name];

        if (argument is null)
            return default;

        if (argument is not IReadOnlyDictionary<string, object?> inputObjectArgumentValue)
            throw new InvalidOperationException("Argument is not an input object");

        var target = new T();
        BindInputObject<T>(inputObjectArgumentValue, target);
        return target;
    }

    public IEnumerable<T?>? BindInputObjectList<T>(ResolverContext context, string name) where T : new()
    {
        var argument = context.Arguments[name];

        if (argument is null)
            return default;

        if (argument is not IEnumerable<IReadOnlyDictionary<string, object?>?> inputObjectArgumentValue)
            throw new InvalidOperationException("Argument is not an input object list");

        var properties = PropertyAdapterFactory.GetPropertyAdapters<T>();

        //todo: do we need the input object fields in here for validation
        // or should the schema of the object be validated already?
        //var inputObjectFields = context.Schema.GetInputFields(name);

        var targetList = new List<T?>();

        foreach (var inputObjectValue in inputObjectArgumentValue)
        {
            if (inputObjectValue is null)
            {
                targetList.Add(default);
                continue;
            }

            var target = new T();
            BindInputObject(inputObjectValue, target);
            targetList.Add(target);
        }

        return targetList;
    }

    public static void BindInputObject<T>(IReadOnlyDictionary<string, object?> inputObject, T target)
    {
        var properties = PropertyAdapterFactory.GetPropertyAdapters<T>();

        //todo: do we need the input object fields in here for validation
        // or should the schema of the object be validated already?
        //var inputObjectFields = context.Schema.GetInputFields(name);

        foreach (var (fieldName, fieldValue) in inputObject)
        {
            var propertyName = FormatPropertyName(fieldName);

            if (properties.TryGetValue(propertyName, out var property))
                property.SetValue(target, fieldValue);
        }
    }

    private static string FormatPropertyName(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        var a = fieldName.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new(a);
    }
}
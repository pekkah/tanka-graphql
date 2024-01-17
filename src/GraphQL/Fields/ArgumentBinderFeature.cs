using Tanka.GraphQL.Features;
using Tanka.GraphQL.Internal;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Fields;

public interface IParseableInputObject
{
    void Parse(IReadOnlyDictionary<string, object?> argumentValue);
}

public class ArgumentBinderFeature : IArgumentBinderFeature
{
    public bool HasArgument(ResolverContextBase context, string name)
    {
        return context.ArgumentValues.ContainsKey(name);
    }

    public T? BindInputObject<T>(ResolverContextBase context, string name)
        where T : new()
    {
        object? argument = context.ArgumentValues[name];

        if (argument is null)
            return default;
        
        if (argument is not IReadOnlyDictionary<string, object?> inputObjectArgumentValue)
            throw new InvalidOperationException("Argument is not an input object");
        
        var target = new T();

        BindInputObject<T>(inputObjectArgumentValue, target);
        return target;
    }

    public IEnumerable<T?>? BindInputObjectList<T>(ResolverContextBase context, string name) where T : new()
    {
        object? argument = context.ArgumentValues[name];

        if (argument is null)
            return default;

        if (argument is not IEnumerable<IReadOnlyDictionary<string, object?>?> inputObjectArgumentValue)
            throw new InvalidOperationException("Argument is not an input object list");

        var targetList = new List<T?>();

        foreach (IReadOnlyDictionary<string, object?>? inputObjectValue in inputObjectArgumentValue)
        {
            if (inputObjectValue is null)
            {
                targetList.Add(default(T));
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
        if (target is IParseableInputObject parseable)
        {
            parseable.Parse(inputObject);
            return;
        }

        IReadOnlyDictionary<string, IPropertyAdapter<T>> properties = PropertyAdapterFactory.GetPropertyAdapters<T>();

        //todo: do we need the input object fields in here for validation
        // or should the schema of the object be validated already?
        //var inputObjectFields = context.Schema.GetInputFields(name);

        foreach ((string fieldName, object? fieldValue) in inputObject)
        {
            string propertyName = FormatPropertyName(fieldName);

            if (properties.TryGetValue(propertyName, out IPropertyAdapter<T>? property))
                property.SetValue(target, fieldValue);
        }
    }

    public T? BindValueArgument<T>(ResolverContextBase context, string name)
    {
        return context.GetArgument<T>(name);
    }

    private static string FormatPropertyName(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        char[] a = fieldName.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }
}
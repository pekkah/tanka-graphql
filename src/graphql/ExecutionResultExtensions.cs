using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL;

public static class ExecutionResultExtensions
{
    public static void AddExtension(this IExecutionResult result, string key, object value)
    {
        if (result.Extensions == null)
        {
            result.Extensions = new Dictionary<string, object>
            {
                { key, value }
            };
            return;
        }

        result.Extensions[key] = value;
    }

    public static object Select(this ExecutionResult er, params object[] path)
    {
        var currentObject = er.Data;
        object result = null;
        foreach (var segment in path)
        {
            if (segment is string stringSegment)
            {
                if (currentObject == null)
                    return null;

                if (currentObject.ContainsKey(stringSegment))
                    result = currentObject[stringSegment];
                else
                    result = null;
            }

            if (segment is int intSegment)
            {
                if (result is IEnumerable enumerable)
                {
                    var count = 0;
                    foreach (var elem in enumerable)
                    {
                        if (count == intSegment)
                        {
                            result = elem;
                            break;
                        }

                        count++;
                    }
                }
                else
                {
                    result = null;
                }
            }

            if (result is Dictionary<string, object> child)
                currentObject = child;
        }

        return result;
    }
}
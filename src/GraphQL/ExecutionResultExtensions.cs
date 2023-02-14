using System.Collections;

namespace Tanka.GraphQL;

public static class SelectExecutionResultExtensions
{
    public static object? Select(this ExecutionResult er, params object[] path)
    {
        var currentObject = er.Data;
        object? result = null;
        foreach (var segment in path)
        {
            if (segment is string stringSegment)
            {
                if (currentObject == null)
                    return null;

                result = currentObject.ContainsKey(stringSegment) ? currentObject[stringSegment] : null;
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

            if (result is IReadOnlyDictionary<string, object?> child)
                currentObject = child;
        }

        return result;
    }
}
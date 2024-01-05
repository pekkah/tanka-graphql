using System.Collections.Concurrent;
using Tanka.GraphQL.Features;

namespace Tanka.GraphQL;

public class ConcurrentBagErrorCollectorFeature(bool includeCode = false, bool includeStackTrace = false)
    : IErrorCollectorFeature
{
    private readonly ConcurrentBag<Exception> _bag = [];

    public void Add(Exception error)
    {
        _bag.Add(error);
    }

    public IEnumerable<ExecutionError> GetErrors()
    {
        return _bag.Select(DefaultFormatError);
    }

    public ExecutionError FormatError(Exception x)
    {
        return DefaultFormatError(x);
    }

    public ExecutionError DefaultFormatError(Exception exception)
    {
        var rootCause = exception.GetBaseException();
        var message = rootCause.Message;
        var error = new ExecutionError
        {
            Message = message
        };

        if (includeCode)
            EnrichWithErrorCode(error, rootCause);

        if (includeStackTrace)
            EnrichWithStackTrace(error, rootCause);

        return exception switch
        {
            FieldException fieldException => FormatFieldException(fieldException, error),
            QueryException queryException => FormatQueryException(queryException, error),
            _ => error
        };
    }

    private static ExecutionError FormatQueryException(QueryException queryException, ExecutionError error)
    {
        error.Path = queryException.Path.Segments.ToArray();

        return error;
    }

    private static ExecutionError FormatFieldException(FieldException fieldException, ExecutionError error)
    {
        error.Locations = new()
        {
            fieldException.Selection.Location ?? default
        };

        error.Path = fieldException.Path.Segments.ToArray();

        return error;
    }

    public static void EnrichWithErrorCode(ExecutionError error, Exception rootCause)
    {
        var code = rootCause.GetType().Name;

        if (code != "Exception")
            code = code.Replace("Exception", string.Empty);

        error.Extend("code", code.ToUpperInvariant());
    }

    public static void EnrichWithStackTrace(ExecutionError error, Exception exception)
    {
        error.Extend("stacktrace", exception.ToString());
    }
}
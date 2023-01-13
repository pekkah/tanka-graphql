using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public partial class Executor
{
    public async Task<ExecutionResult> ExecuteQueryAsync(QueryContext context)
    {
        var path = new NodePath();
        var queryType = context.Schema.Query;

        if (queryType == null)
            throw new QueryException("Schema does not support queries. Query type is null.")
            {
                Path = path
            };

        var selectionSet = context.OperationDefinition.SelectionSet;

        try
        {
            var result = await context.ExecuteSelectionSet(
                selectionSet,
                queryType,
                context.Request.InitialValue,
                path);

            return new ExecutionResult
            {
                Data = result,
                Errors = context.ErrorCollector.GetErrors().Select(e => new ExecutionError()
                {
                    Path = (e as FieldException)?.Path.Segments.ToList() ?? path.Segments.ToList(),
                    Message = e.Message,
                    Extensions = new Dictionary<string, object>()
                    {
                        ["ExceptionType"] = e.GetBaseException().GetType().Name,
                        ["StackTrace"] = e.StackTrace ?? string.Empty
                    }
                }).ToList()
            };
        }
        catch (FieldException e)
        {
            context.ErrorCollector.Add(e);
        }

        return new ExecutionResult()
        {
            Data = null,
            Errors = context.ErrorCollector.GetErrors().Select(e => new ExecutionError()
            {
                Path = (e as FieldException)?.Path.Segments.ToList() ?? path.Segments.ToList(),
                Message = e.Message,
                Extensions = new Dictionary<string, object>()
                {
                    ["ExceptionType"] = e.GetBaseException().GetType().Name,
                    ["StackTrace"] = e.StackTrace ?? string.Empty
                }
            }).ToList()
        };
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental;

public static class FieldExecutorQueryContextExtensions
{
    public static Task<object?> ExecuteField(
        this QueryContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path)
    {
        return context.FieldExecutor.Execute(context, objectDefinition, objectValue, fields, path);
    }
}
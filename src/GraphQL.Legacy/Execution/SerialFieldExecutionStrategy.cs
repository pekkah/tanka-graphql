using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Execution;

public class SerialFieldExecutionStrategy : IExecutionStrategy
{
    public async Task<IDictionary<string, object?>?> ExecuteSelectionSetAsync(
        IExecutorContext executorContext, 
        SelectionSet selectionSet,
        ObjectDefinition objectDefinition, 
        object objectValue, 
        NodePath path)
    {
        if (executorContext == null) throw new ArgumentNullException(nameof(executorContext));
        if (selectionSet == null) throw new ArgumentNullException(nameof(selectionSet));
        if (path == null) throw new ArgumentNullException(nameof(path));

        var groupedFieldSet = SelectionSets.CollectFields(
            executorContext.Schema,
            executorContext.Document,
            objectDefinition,
            selectionSet,
            executorContext.CoercedVariableValues);


        return null;
    }
}

public interface IQueryNode
{
    IQueryNode Execute();
}
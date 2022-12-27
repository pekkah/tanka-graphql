using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.Planner;

public class QueryExecutorFacts
{
    [Fact]
    public async Task Execute_root_field()
    {
        /* Given */
        var schema = await new SchemaBuilder()
            .Add("""
            type Query 
            {
                version: String!
            }
            """)
            .Build(new SchemaBuildOptions());

        ExecutableDocument query = """
            {
                version
            }
            """;

        /* When */


        /* Then */

    }
}

public class NonRecursiveExecutor
{
    public virtual async Task<IDictionary<string, object?>?> ExecuteSelectionSetAsync(
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

        var resultMap = new Dictionary<string, object?>(groupedFieldSet.Count);

        foreach (var (responseKey, fields) in groupedFieldSet)
        {
            var node = BuildFieldNode(
                executorContext,
                objectDefinition,
                objectValue,
                fields,
                path.Fork());
        }

        return resultMap;
    }

    private IFieldResult BuildFieldNode(IExecutorContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path)
    {
        var schema = context.Schema;
        var fieldSelection = fields.First();
        var fieldName = fieldSelection.Name;
        var field = schema.GetField(objectDefinition.Name, fieldName);
        
        object? completedValue = null;

        if (field is null)
            return new ValueResult() { Value = completedValue};

        var argumentValues = ArgumentCoercion.CoerceArgumentValues(
            schema,
            objectDefinition,
            fieldSelection,
            context.CoercedVariableValues);

        try
        {
            var resolver = schema.GetResolver(objectDefinition.Name, fieldName);

            if (resolver == null)
                throw new QueryExecutionException(
                    $"Could not get resolver for {objectDefinition.Name}.{fieldName}",
                    path);

            var resolverContext =
                new ResolverContext(
                    objectDefinition,
                    objectValue,
                    field,
                    fieldSelection,
                    fields,
                    argumentValues,
                    path,
                    context);

            // begin resolve
            await context.ExtensionsRunner.BeginResolveAsync(resolverContext);
            var resultTask = resolver(resolverContext);

            IResolverResult result;
            if (resultTask.IsCompletedSuccessfully)
                result = resultTask.Result;
            else
                result = await resultTask;

            await context.ExtensionsRunner.EndResolveAsync(resolverContext, result);
            // end resolve

            // begin complete
            var completedValueTask = result.CompleteValueAsync(resolverContext);
            if (completedValueTask.IsCompletedSuccessfully)
                completedValue = completedValueTask.Result;
            else
                completedValue = await completedValueTask;
            // end complete

            return completedValue;
        }
        catch (Exception e)
        {
            /*return FieldErrors.Handle(
                context,
                objectDefinition,
                fieldName,
                fieldType,
                fieldSelection,
                completedValue,
                e,
                path);*/
            throw;
        }
    }
}

public interface IFieldResult
{
}

public class ValueResult: IFieldResult
{
    public object? Value { get; set; }
}

public class SelectionSetResult: IFieldResult
{
    public SelectionSet SelectionSet { get; set; }

    public ObjectDefinition ObjectDefinition { get; set; }

    public object? ObjectValue { get; set; }

    public NodePath Path { get; set; }
}
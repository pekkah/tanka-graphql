using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using NSubstitute.Core;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Xunit;
using System.IO;
using Tanka.GraphQL.Introspection;

namespace Tanka.GraphQL.Tests.Planner;

public class QueryPlanExecutorFacts
{
    [Fact]
    public async Task Plan()
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
        var plan = new QueryPlanner(schema).Plan(query);
        var result = new QueryPlanExecutor().Execute(plan);

        /* Then */

    }
}

public class QueryPlanExecutorContext
{
    public void Add(IExecutionResult result)
    {

    }
}

public class QueryPlanExecutor
{
    public async IAsyncEnumerable<ExecutionResult> Execute(QueryPlan plan)
    {
        var root = plan.Root;

        var nodes = new Queue<QueryPlanNode>();
        nodes.Enqueue(root);

        while (nodes.TryDequeue(out var node))
        {
            var task = node switch
            {
                SelectionSetNode objectNode => ExecuteObjectNode(plan, objectNode),
                _ => throw new InvalidOperationException("Root should be object?")
            };

            await task;
        }

        yield break;
    }

    private Task ExecuteObjectNode(QueryPlan plan, SelectionSetNode selectionSetNode)
    {
        return null;
    }
}

public class QueryPlanner
{
    public ISchema Schema { get; }

    public QueryPlanner(ISchema schema)
    {
        Schema = schema;
    }

    public QueryPlan Plan(ExecutableDocument document, string operationName = null)
    {
        var operation = Operations.GetOperation(document, operationName);

        return operation.Operation switch
        {
            OperationType.Query => PlanQueryOperation(document, operation),
            //OperationType.Mutation => expr,
            //OperationType.Subscription => expr,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private QueryPlan PlanQueryOperation(ExecutableDocument document, OperationDefinition operation)
    {
        var path = new NodePath();
        var objectDefinition = Schema.Query;
        var selectionset = operation.SelectionSet;
        var groupedFieldSet = SelectionSets.CollectFields(
            Schema,
            document,
            objectDefinition,
            selectionset);

        var children = new List<QueryPlanNode>();

        foreach (var (responseKey, fields) in groupedFieldSet)
        {
            children.Add(PlanField(responseKey, fields, objectDefinition, path.Fork()));
        }

        return new QueryPlan(null);
    }

    private QueryPlanNode PlanField(
        string responseKey, 
        IReadOnlyList<FieldSelection> fields, 
        ObjectDefinition objectDefinition, 
        NodePath path)
    {
        var fieldName = fields.First().Name;
        path.Append(fieldName);

        // __typename hack
        //if (fieldName == "__typename") return null;

        var field = Schema
            .GetField(objectDefinition.Name, fieldName);

        if (field is not null)
        {
            var fieldType = field.Type;
            var resolver = Schema.GetResolver(objectDefinition.Name, fieldName);

            //todo: do I create somekind of russian doll model for the definition as it an be List, NonNull etc
            //var fieldTypeDefinition = ???
        }

        return null;
    }
}

public record QueryPlan(QueryPlanNode Root);

public class QueryPlanNode
{
    public QueryPlanNode[] Children { get; set; }
}

public class SelectionSetNode : QueryPlanNode
{
    public Task Execute(object? objectValue)
    {
        return Task.CompletedTask;
    }

    public SelectionSet SelectionSet { get; set; }
    public ObjectDefinition ObjectDefinition { get; set; }
    public IReadOnlyDictionary<string, List<FieldSelection>> GroupedFieldSet { get; set; }
}
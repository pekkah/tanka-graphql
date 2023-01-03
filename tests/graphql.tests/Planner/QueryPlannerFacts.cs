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
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.ValueResolution;
using FieldDefinition = Tanka.GraphQL.Language.Nodes.TypeSystem.FieldDefinition;
using TypeDefinition = Tanka.GraphQL.Language.Nodes.TypeSystem.TypeDefinition;
using System.Xml.Linq;

namespace Tanka.GraphQL.Tests.Planner;

public class QueryPlanExecutorFacts
{
    [Fact]
    public async Task Simple_Scalar()
    {
        /* Given */
        var schema = await new SchemaBuilder()
            .Add("""
            type Query 
            {
                version: String!
            }
            """)
            .Build(new ResolversMap()
            {
                ["Query"] = new()
                {
                    {"version", ctx => ResolveSync.As("1.0")}
                }
            });

        ExecutableDocument query = """
            {
                version
            }
            """;

        /* When */
        var plan = new QueryPlanner(schema).Plan(query);
        await using var enumerator = new QueryPlanExecutor(schema)
            .Execute(plan, null)
            .GetAsyncEnumerator();

        await enumerator.MoveNextAsync();
        var result = enumerator.Current;

        /* Then */
        result.ShouldMatchJson("""
            {
               "data": {
                  "version": "1.0"
              }
            }
            """);
    }

    [Fact]
    public async Task Object_with_ScalarField()
    {
        /* Given */
        var schema = await new SchemaBuilder()
            .Add("""
            type System 
            {
                version: String!
            }

            type Query 
            {
                system: System!
            }
            """)
            .Build(new ResolversMap()
            {
                ["Query"] = new()
                {
                    {"system", ctx => ResolveSync.As("system")}
                },
                ["System"] = new()
                {
                    {"version", ctx => ResolveSync.As("1.0")}
                }
            });

        ExecutableDocument query = """
            {
                system {
                    version
                }
            }
            """;

        /* When */
        var plan = new QueryPlanner(schema).Plan(query);
        await using var enumerator = new QueryPlanExecutor(schema)
            .Execute(plan, null)
            .GetAsyncEnumerator();

        await enumerator.MoveNextAsync();
        var result = enumerator.Current;

        /* Then */
        result.ShouldMatchJson("""
            {
               "data": {
                  "version": "1.0"
              }
            }
            """);
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
    public ISchema Schema { get; }

    public QueryPlanExecutor(ISchema schema)
    {
        Schema = schema;
    }

    public async IAsyncEnumerable<ExecutionResult> Execute(
        QueryPlan plan,
        object? initialValue)
    {
        var root = plan.Root;

        var nodes = new Queue<SelectionSetNode>();
        nodes.Enqueue(root.RootSelectionSetNode);

        var resultMap = new Dictionary<string, object>();
        var selectionSetResultMap = resultMap;
        while (nodes.TryDequeue(out var selectionSetNode))
        {
            foreach (var node in selectionSetNode.Children)
            {
                switch (node)
                {
                    case ScalarNode scalarNode:
                        var result = await ExecuteScalarNode(
                            selectionSetNode,
                            scalarNode,
                            initialValue,
                            new Dictionary<string, object>());
                        selectionSetResultMap[scalarNode.ResponseKey] = result;
                    break;
                    case SelectionSetNode subSelectionSetNode:
                        nodes.Enqueue(subSelectionSetNode);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        
        yield return new ExecutionResult()
        {
            Data = resultMap
        };
    }

    private async Task<object> ExecuteScalarNode(
        SelectionSetNode selectionSetNode, 
        ScalarNode scalarNode, 
        object? objectValue,
        Dictionary<string, object> coercedVariableValues)
    {
        var fieldSelection = scalarNode.Fields.First();
        var argumentValues = ArgumentCoercion.CoerceArgumentValues(
            Schema,
            selectionSetNode.ObjectDefinition,
            scalarNode.Fields.First(),
            coercedVariableValues);

        var resolver = Schema.GetResolver(
            selectionSetNode.ObjectDefinition.Name,
            scalarNode.FieldName);

        if (resolver is null)
            return null;

        var resolverContext = new ResolverContext(
            selectionSetNode.ObjectDefinition,
            objectValue,
            scalarNode.FieldDefinition,
            fieldSelection,
            scalarNode.Fields!,
            argumentValues,
            scalarNode.Path,
            null!); //todo: we need alternative for this?

        var resolvedValue = await resolver(resolverContext);

        return resolvedValue.Value;
    }

    private Task ExecuteObjectNode(
        QueryPlan plan, 
        SelectionSetNode selectionSetNode)
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
            children.Add(PlanField(document, responseKey, fields, objectDefinition, path.Fork()));
        }

        return new QueryPlan(new RootNode()
        {
            Path = path,
            ObjectDefinition = objectDefinition,
            OperationDefinition = operation,
            RootSelectionSetNode = new SelectionSetNode()
            {
                Children = children,
                GroupedFieldSet = groupedFieldSet,
                ObjectDefinition = objectDefinition,
                SelectionSet = selectionset
            }
        });
    }

    private QueryPlanNode PlanField(
        ExecutableDocument document, 
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
            var fieldTypeDefinition = Ast.UnwrapAndResolveType(Schema, fieldType);
            var isComplexType = fieldTypeDefinition?.Kind is NodeKind.ObjectDefinition or NodeKind.InterfaceDefinition or NodeKind.UnionDefinition;

            if (!isComplexType)
            {
                return new ScalarNode()
                {
                    Path = path,
                    FieldDefinition = field,
                    UnwrappedFieldType = fieldTypeDefinition,
                    ObjectDefinition = objectDefinition,
                    FieldName = fieldName,
                    Fields = fields,
                    ResponseKey = responseKey,
                };
            }

            var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
            var groupedFieldSet = SelectionSets.CollectFields(
                Schema,
                document,
                objectDefinition,
                subSelectionSet);

            var children = new List<QueryPlanNode>();

            foreach (var (subResponseKey, subFields) in groupedFieldSet)
            {
                children.Add(PlanField(
                    document,
                    subResponseKey, 
                    subFields, 
                    fieldTypeDefinition as ObjectDefinition, 
                    path.Fork()));
            }

            return new SelectionSetNode()
            {
                Children = children,
                GroupedFieldSet = groupedFieldSet,
                ObjectDefinition = fieldTypeDefinition as ObjectDefinition,
                SelectionSet = subSelectionSet
            };

        }

        return null;
    }
}

public record QueryPlan(RootNode Root);

public class QueryPlanNode
{
}

public class ScalarNode : QueryPlanNode
{
    public NodePath Path { get; set; }
    
    public FieldDefinition FieldDefinition { get; set; }
    
    public TypeDefinition UnwrappedFieldType { get; set; }
    
    public ObjectDefinition ObjectDefinition { get; set; }
    
    public string FieldName { get; set; }
    
    public IReadOnlyList<FieldSelection> Fields { get; set; }
    
    public string ResponseKey { get; set; }
}

public class SelectionSetNode : QueryPlanNode
{
    public SelectionSet SelectionSet { get; set; }
    
    public ObjectDefinition ObjectDefinition { get; set; }
    
    public IReadOnlyDictionary<string, List<FieldSelection>> GroupedFieldSet { get; set; }

    public IReadOnlyList<QueryPlanNode> Children { get; set; }
}

public class RootNode : QueryPlanNode
{
    public NodePath Path { get; set; }
    
    public ObjectDefinition ObjectDefinition { get; set; }
    
    public OperationDefinition OperationDefinition { get; set; }

    public SelectionSetNode RootSelectionSetNode { get; set; }
}
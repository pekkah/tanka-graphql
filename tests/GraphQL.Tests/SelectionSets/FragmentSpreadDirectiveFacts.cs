using System;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.SelectionSets;

public class FragmentSpreadDirectiveFacts
{
    private readonly ISchema _schema;
    private readonly IServiceProvider _serviceProvider;

    public FragmentSpreadDirectiveFacts()
    {
        _schema = new ExecutableSchemaBuilder()
            .Add("Query", new()
            {
                { "field1: String", b => b.ResolveAs("value1") },
                { "field2: String", b => b.ResolveAs("value2") },
                { "deferredField: String", b => b.ResolveAs("deferred") }
            })
            .Build()
            .GetAwaiter()
            .GetResult();

        var services = new ServiceCollection();
        services.AddSingleton<IFieldCollector, DefaultFieldCollector>();
        services.AddKeyedSingleton<IDirectiveHandler>("skip", new SkipDirectiveHandler());
        services.AddKeyedSingleton<IDirectiveHandler>("include", new IncludeDirectiveHandler());
        services.AddKeyedSingleton<IDirectiveHandler>("defer", new DeferDirectiveHandler());
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void CollectFields_fragment_spread_with_defer_directive()
    {
        // Given - @defer on fragment spread
        ExecutableDocument document = """
            query {
                field1
                ...TestFragment @defer
            }
            fragment TestFragment on Query {
                deferredField
            }
            """;
            
        var fieldCollector = _serviceProvider.GetRequiredService<IFieldCollector>();
        var queryType = _schema.Query!;
        var selectionSet = document.OperationDefinitions[0].SelectionSet;

        // When
        var result = fieldCollector.CollectFields(
            _schema,
            document,
            queryType,
            selectionSet);

        // Then - both fields should be collected
        Assert.Equal(2, result.Fields.Count);
        Assert.Contains("field1", result.Fields.Keys);
        Assert.Contains("deferredField", result.Fields.Keys);
        
        // And the deferred field should have @defer metadata
        Assert.NotNull(result.FieldMetadata);
        Assert.True(result.FieldMetadata.ContainsKey("deferredField"));
        Assert.True(result.FieldMetadata["deferredField"].ContainsKey("defer"));
    }

    [Fact]
    public void CollectFields_fragment_spread_with_skip_directive()
    {
        // Given - @skip(if: true) on fragment spread
        ExecutableDocument document = """
            query {
                field1
                ...TestFragment @skip(if: true)
            }
            fragment TestFragment on Query {
                field2
            }
            """;
            
        var fieldCollector = _serviceProvider.GetRequiredService<IFieldCollector>();
        var queryType = _schema.Query!;
        var selectionSet = document.OperationDefinitions[0].SelectionSet;

        // When
        var result = fieldCollector.CollectFields(
            _schema,
            document,
            queryType,
            selectionSet);

        // Then - only field1 should be collected, field2 should be skipped
        Assert.Single(result.Fields);
        Assert.Contains("field1", result.Fields.Keys);
        Assert.DoesNotContain("field2", result.Fields.Keys);
    }

    [Fact]
    public void CollectFields_fragment_spread_with_include_directive_false()
    {
        // Given - @include(if: false) on fragment spread
        ExecutableDocument document = """
            query {
                field1
                ...TestFragment @include(if: false)
            }
            fragment TestFragment on Query {
                field2
            }
            """;
            
        var fieldCollector = _serviceProvider.GetRequiredService<IFieldCollector>();
        var queryType = _schema.Query!;
        var selectionSet = document.OperationDefinitions[0].SelectionSet;

        // When
        var result = fieldCollector.CollectFields(
            _schema,
            document,
            queryType,
            selectionSet);

        // Then - only field1 should be collected, field2 should be excluded
        Assert.Single(result.Fields);
        Assert.Contains("field1", result.Fields.Keys);
        Assert.DoesNotContain("field2", result.Fields.Keys);
    }

    [Fact]
    public void CollectFields_fragment_spread_with_include_directive_true()
    {
        // Given - @include(if: true) on fragment spread
        ExecutableDocument document = """
            query {
                field1
                ...TestFragment @include(if: true)
            }
            fragment TestFragment on Query {
                field2
            }
            """;
            
        var fieldCollector = _serviceProvider.GetRequiredService<IFieldCollector>();
        var queryType = _schema.Query!;
        var selectionSet = document.OperationDefinitions[0].SelectionSet;

        // When
        var result = fieldCollector.CollectFields(
            _schema,
            document,
            queryType,
            selectionSet);

        // Then - both fields should be collected
        Assert.Equal(2, result.Fields.Count);
        Assert.Contains("field1", result.Fields.Keys);
        Assert.Contains("field2", result.Fields.Keys);
    }

    [Fact]
    public void CollectFields_fragment_spread_without_directives()
    {
        // Given - fragment spread without directives
        ExecutableDocument document = """
            query {
                field1
                ...TestFragment
            }
            fragment TestFragment on Query {
                field2
            }
            """;
            
        var fieldCollector = _serviceProvider.GetRequiredService<IFieldCollector>();
        var queryType = _schema.Query!;
        var selectionSet = document.OperationDefinitions[0].SelectionSet;

        // When
        var result = fieldCollector.CollectFields(
            _schema,
            document,
            queryType,
            selectionSet);

        // Then - both fields should be collected
        Assert.Equal(2, result.Fields.Count);
        Assert.Contains("field1", result.Fields.Keys);
        Assert.Contains("field2", result.Fields.Keys);
    }
}
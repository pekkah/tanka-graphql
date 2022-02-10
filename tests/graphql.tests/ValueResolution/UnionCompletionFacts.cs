﻿using System.Threading.Tasks;
using NSubstitute;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution;

public class UnionCompletionFacts
{
    [Fact]
    public async Task Should_fail_if_no_ActualType_given()
    {
        /* Given */
        var schema = await new SchemaBuilder()
            .Add(@"
type Success
type Failure
union Result = Success | Failure
type Query
"
            ).Build(new SchemaBuildOptions());
        var value = new object();
        var context = Substitute.For<IResolverContext>();
        context.ExecutionContext.Schema.Returns(schema);
        context.FieldName.Returns("field");
        context.Field.Returns("field: Result");


        var sut = new CompleteValueResult(value, null);

        /* When */
        var exception =
            await Assert.ThrowsAsync<CompleteValueException>(() => sut.CompleteValueAsync(context).AsTask());

        /* Then */
        Assert.Equal("Cannot complete value for field 'field: Result'. ActualType is required for union values.",
            exception.Message);
    }

    [Fact]
    public async Task Should_fail_if_ActualType_is_not_possible()
    {
        /* Given */
        ObjectDefinition notPossible = "type NotPossible";
        var schema = await new SchemaBuilder()
            .Add(@"
type Success
type Failure
union Result = Success | Failure
type Query
"
            ).Build(new SchemaBuildOptions());
        var value = new object();
        var context = Substitute.For<IResolverContext>();
        context.ExecutionContext.Schema.Returns(schema);
        context.FieldName.Returns("field");
        context.Field.Returns("field: Result");


        var sut = new CompleteValueResult(value, notPossible);

        /* When */
        var exception =
            await Assert.ThrowsAsync<CompleteValueException>(() => sut.CompleteValueAsync(context).AsTask());

        /* Then */
        Assert.Equal(
            "Cannot complete value for field 'field: Result'. ActualType 'NotPossible' is not possible for 'Result'",
            exception.Message);
    }

    [Fact]
    public async Task Should_complete_value()
    {
        /* Given */
        var schema = await new SchemaBuilder()
            .Add(@"
type Success
type Failure
union Result = Success | Failure
type Query
"
            ).Build(new SchemaBuildOptions());
        var mockValue = new object();
        var context = Substitute.For<IResolverContext>();
        context.ExecutionContext.Schema.Returns(schema);
        context.ExecutionContext.Document.Returns(new ExecutableDocument(null, null));
        context.Path.Returns(new NodePath());
        context.FieldName.Returns("field");
        context.Field.Returns("field: Result");


        var sut = new CompleteValueResult(mockValue, "type Success");

        /* When */
        var value = await sut.CompleteValueAsync(context);

        /* Then */
        Assert.NotNull(value);
    }

    [Fact]
    public async Task Should_complete_list_of_values()
    {
        /* Given */
        var schema = await new SchemaBuilder()
            .Add(@"
type Success
type Failure
union Result = Success | Failure
type Query
"
            ).Build(new SchemaBuildOptions());

        var mockValue = new object();
        var context = Substitute.For<IResolverContext>();
        context.ExecutionContext.Schema.Returns(schema);
        context.ExecutionContext.Document.Returns(new ExecutableDocument(null, null));
        context.Path.Returns(new NodePath());
        context.FieldName.Returns("field");
        context.Field.Returns("field: Result");


        var sut = new CompleteValueResult(new[] { mockValue }, (_, _) => "type Success");

        /* When */
        var value = await sut.CompleteValueAsync(context);

        /* Then */
        Assert.NotNull(value);
    }
}
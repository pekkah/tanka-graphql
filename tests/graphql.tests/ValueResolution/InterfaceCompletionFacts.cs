﻿using System.Collections.Generic;
using System.Threading.Tasks;

using NSubstitute;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution
{
    public class InterfaceCompletionFacts
    {
        [Fact]
        public async Task Should_fail_if_no_ActualType_given()
        {
            /* Given */
            var character = new InterfaceType("Character");
            var value = new object();
            var context = Substitute.For<IResolverContext>();
            context.FieldName.Returns("field");
            context.Field.Returns(new Field(character));


            var sut = new CompleteValueResult(value, null);

            /* When */
            var exception =
                await Assert.ThrowsAsync<CompleteValueException>(() => sut.CompleteValueAsync(context).AsTask());

            /* Then */
            Assert.Equal(
                "Cannot complete value for field 'field':'Character'. ActualType is required for interface values.",
                exception.Message);
        }

        [Fact]
        public async Task Should_fail_if_ActualType_is_not_possible()
        {
            /* Given */
            var character = new InterfaceType("Character");
            var humanNotCharacter = new ObjectType("Human");
            var value = new object();
            var context = Substitute.For<IResolverContext>();
            context.FieldName.Returns("field");
            context.Field.Returns(new Field(character));


            var sut = new CompleteValueResult(value, humanNotCharacter);

            /* When */
            var exception =
                await Assert.ThrowsAsync<CompleteValueException>(() => sut.CompleteValueAsync(context).AsTask());

            /* Then */
            Assert.Equal(
                "Cannot complete value for field 'field':'Character'. ActualType 'Human' does not implement interface 'Character'",
                exception.Message);
        }

        [Fact]
        public async Task Should_complete_value()
        {
            /* Given */
            var character = new InterfaceType("Character");
            var humanCharacter = new ObjectType("Human", implements: new[] {character});
            var mockValue = new object();
            var context = Substitute.For<IResolverContext>();
            context.ExecutionContext.Schema.Returns(Substitute.For<ISchema>());
            context.ExecutionContext.Document.Returns(new ExecutableDocument(null, null));
            context.Path.Returns(new NodePath());
            context.FieldName.Returns("field");
            context.Field.Returns(new Field(character));


            var sut = new CompleteValueResult(mockValue, humanCharacter);

            /* When */
            var value = await sut.CompleteValueAsync(context);

            /* Then */
            Assert.NotNull(value);
        }

        [Fact]
        public async Task Should_complete_list_of_values()
        {
            /* Given */
            var character = new InterfaceType("Character");
            var humanCharacter = new ObjectType("Human", implements: new[] {character});
            var mockValue = new object();
            var context = Substitute.For<IResolverContext>();
            context.ExecutionContext.Schema.Returns(Substitute.For<ISchema>());
            context.ExecutionContext.Document.Returns(new ExecutableDocument(null, null));
            context.Path.Returns(new NodePath());
            context.FieldName.Returns("field");
            context.Field.Returns(new Field(character));


            var sut = new CompleteValueResult(new[] {mockValue}, _ => humanCharacter);

            /* When */
            var value = await sut.CompleteValueAsync(context);

            /* Then */
            Assert.NotNull(value);
        }
    }
}
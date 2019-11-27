using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;
using NSubstitute;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution
{
    public class UnionCompletionFacts
    {
        [Fact]
        public async Task Should_fail_if_no_ActualType_given()
        {
            /* Given */
            var success = new ObjectType("Success");
            var error = new ObjectType("Error");
            var result = new UnionType("Result", new []{success, error});
            var value = new object();
            var context = Substitute.For<IResolverContext>();
            context.FieldName.Returns("field");
            context.Field.Returns(new Field(result));


            var sut = new CompleteValueResult(value, null);

            /* When */
            var exception = await Assert.ThrowsAsync<CompleteValueException>(()=> sut.CompleteValueAsync(context).AsTask());

            /* Then */
            Assert.Equal("Cannot complete value for field 'field':'Result'. ActualType is required for union values.", exception.Message);
        }

        [Fact]
        public async Task Should_fail_if_ActualType_is_not_possible()
        {
            /* Given */
            var notPossible = new ObjectType("NotPossible");
            var success = new ObjectType("Success");
            var error = new ObjectType("Error");
            var result = new UnionType("Result", new []{success, error});
            var value = new object();
            var context = Substitute.For<IResolverContext>();
            context.FieldName.Returns("field");
            context.Field.Returns(new Field(result));


            var sut = new CompleteValueResult(value, notPossible);

            /* When */
            var exception = await Assert.ThrowsAsync<CompleteValueException>(()=> sut.CompleteValueAsync(context).AsTask());

            /* Then */
            Assert.Equal("Cannot complete value for field 'field':'Result'. ActualType 'NotPossible' is not possible for 'Result'", exception.Message);
        }

        [Fact]
        public async Task Should_complete_value()
        {
            /* Given */
            var success = new ObjectType("Success");
            var error = new ObjectType("Error");
            var result = new UnionType("Result", new []{success, error});
            var mockValue = new object();
            var context = Substitute.For<IResolverContext>();
            context.ExecutionContext.Schema.Returns(Substitute.For<ISchema>());
            context.ExecutionContext.Document.Returns(new GraphQLDocument()
            {
                Definitions = new List<ASTNode>()
            });
            context.Path.Returns(new NodePath());
            context.FieldName.Returns("field");
            context.Field.Returns(new Field(result));


            var sut = new CompleteValueResult(mockValue, success);

            /* When */
            var value = await sut.CompleteValueAsync(context);

            /* Then */
            Assert.NotNull(value);
        }

        [Fact]
        public async Task Should_complete_list_of_values()
        {
            /* Given */
            var success = new ObjectType("Success");
            var error = new ObjectType("Error");
            var result = new UnionType("Result", new []{success, error});
            var mockValue = new object();
            var context = Substitute.For<IResolverContext>();
            context.ExecutionContext.Schema.Returns(Substitute.For<ISchema>());
            context.ExecutionContext.Document.Returns(new GraphQLDocument()
            {
                Definitions = new List<ASTNode>()
            });
            context.Path.Returns(new NodePath());
            context.FieldName.Returns("field");
            context.Field.Returns(new Field(result));


            var sut = new CompleteValueResult(new [] {mockValue} , _ => success);

            /* When */
            var value = await sut.CompleteValueAsync(context);

            /* Then */
            Assert.NotNull(value);
        }
    }
}
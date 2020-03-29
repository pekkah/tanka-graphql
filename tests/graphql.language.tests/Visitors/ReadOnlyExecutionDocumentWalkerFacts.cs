using NSubstitute;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Visitors;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Visitors
{
    public class ReadOnlyExecutionDocumentWalkerFacts
    {
        [Fact]
        public void ExecutableDocument_EnterAndLeave()
        {
            /* Given */
            ExecutableDocument document = @"";

            var visitor = Substitute.For<VisitAllBase>();
            var sut = new ReadOnlyExecutionDocumentWalker(
                new ExecutionDocumentWalkerOptions().Add(visitor));

            /* When */
            sut.Visit(document);

            /* Then */
            visitor.Received().Enter(document);
            visitor.Received().Leave(document);
        }

        [Fact]
        public void FieldSelection_EnterAndLeave()
        {
            /* Given */
            var selection = Parser.Create("field").ParseFieldSelection();

            var visitor = Substitute.For<VisitAllBase>();
            var sut = new ReadOnlyExecutionDocumentWalker(
                new ExecutionDocumentWalkerOptions().Add(visitor));

            /* When */
            sut.Visit(selection);

            /* Then */
            visitor.Received().Enter(selection);
            visitor.Received().Leave(selection);
        }

        [Fact]
        public void FragmentDefinition_EnterAndLeave()
        {
            /* Given */
            FragmentDefinition definition = @"fragment Name on Human { field }";

            var visitor = Substitute.For<VisitAllBase>();
            var sut = new ReadOnlyExecutionDocumentWalker(
                new ExecutionDocumentWalkerOptions().Add(visitor));

            /* When */
            sut.Visit(definition);

            /* Then */
            visitor.Received().Enter(definition);
            visitor.Received().Leave(definition);
        }

        [Fact]
        public void FragmentSpread_EnterAndLeave()
        {
            /* Given */
            var selection = Parser.Create("...name").ParseFragmentSpread();

            var visitor = Substitute.For<VisitAllBase>();
            var sut = new ReadOnlyExecutionDocumentWalker(
                new ExecutionDocumentWalkerOptions().Add(visitor));

            /* When */
            sut.Visit(selection);

            /* Then */
            visitor.Received().Enter(selection);
            visitor.Received().Leave(selection);
        }

        [Fact]
        public void InlineFragment_EnterAndLeave()
        {
            /* Given */
            var selection = Parser.Create("...{ field }").ParseInlineFragment();

            var visitor = Substitute.For<VisitAllBase>();
            var sut = new ReadOnlyExecutionDocumentWalker(
                new ExecutionDocumentWalkerOptions().Add(visitor));

            /* When */
            sut.Visit(selection);

            /* Then */
            visitor.Received().Enter(selection);
            visitor.Received().Leave(selection);
        }

        [Fact]
        public void OperationDefinition_EnterAndLeave()
        {
            /* Given */
            OperationDefinition definition = @"{ field }";

            var visitor = Substitute.For<VisitAllBase>();
            var sut = new ReadOnlyExecutionDocumentWalker(
                new ExecutionDocumentWalkerOptions().Add(visitor));

            /* When */
            sut.Visit(definition);

            /* Then */
            visitor.Received().Enter(definition);
            visitor.Received().Leave(definition);
        }

        [Fact]
        public void SelectionSet_EnterAndLeave()
        {
            /* Given */
            var definition = Parser.Create("{ field }").ParseSelectionSet();

            var visitor = Substitute.For<VisitAllBase>();
            var sut = new ReadOnlyExecutionDocumentWalker(
                new ExecutionDocumentWalkerOptions().Add(visitor));

            /* When */
            sut.Visit(definition);

            /* Then */
            visitor.Received().Enter(definition);
            visitor.Received().Leave(definition);
        }
    }
}
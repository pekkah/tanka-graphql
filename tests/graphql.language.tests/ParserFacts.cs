using System.Linq;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Language.Tests
{
    public class ParserFacts
    {
        [Fact]
        public void OperationDefinition_Empty()
        {
            /* Given */
            var source = "query { }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.Parse();

            /* Then */
            Assert.Single(actual.OperationDefinitions);
        }

        [Fact]
        public void OperationDefinition_SelectionSet_Selection()
        {
            /* Given */
            var source = "query { field }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.Parse();

            /* Then */
            Assert.Single(actual.OperationDefinitions);
            Assert.Single(actual.OperationDefinitions.Single().SelectionSet.Selections);
        }

        [Fact]
        public void OperationDefinition_SelectionSet_Field()
        {
            /* Given */
            var source = "query { field }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.Parse();

            /* Then */
            var selectionSet = actual.OperationDefinitions
                .Single()
                .SelectionSet;

            Assert.Single(
                selectionSet.Selections.OfType<FieldSelection>(),
                selection => selection.Name == "field"
                             && selection.Alias == "alias");
        }

        [Fact]
        public void OperationDefinition_SelectionSet_Field_with_Alias()
        {
            /* Given */
            var source = "query { alias: field }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.Parse();

            /* Then */
            var selectionSet = actual.OperationDefinitions
                .Single()
                .SelectionSet;

            Assert.Single(
                selectionSet.Selections.OfType<FieldSelection>(),
                selection => selection.Name == "field"
                && selection.Alias == "alias");
        }
    }
}
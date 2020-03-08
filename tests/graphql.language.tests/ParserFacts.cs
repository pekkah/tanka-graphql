using System.Text;
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
    }
}
using tanka.graphql.type;
using GraphQLParser.AST;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class EnumTypeFacts
    {
        [Theory]
        [InlineData("success", "SUCCESS")]
        [InlineData("FAILURE", "FAILURE")]
        [InlineData("Inconclusive", "INCONCLUSIVE")]
        public void ParseValue(object input, string expected)
        {
            /* Given */
            var Enum = new EnumType("TestResult", new EnumValues()
            {
                ["SUCCESS"] = null,
                ["FAILURE"] = null,
                ["INCONCLUSIVE"] = null
            });

            /* When */
            var actual = Enum.ParseValue(input);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("success", "SUCCESS")]
        [InlineData("FAILURE", "FAILURE")]
        [InlineData("Inconclusive", "INCONCLUSIVE")]
        public void Serialize(object input, string expected)
        {
            /* Given */
            var Enum = new EnumType("TestResult", new EnumValues()
            {
                ["SUCCESS"] = null,
                ["FAILURE"] = null,
                ["INCONCLUSIVE"] = null
            });

            /* When */
            var actual = Enum.Serialize(input);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("success", "SUCCESS")]
        [InlineData("FAILURE", "FAILURE")]
        [InlineData("Inconclusive", "INCONCLUSIVE")]
        public void ParseLiteral(string input, string expected)
        {
            /* Given */
            var astValue = new GraphQLScalarValue(ASTNodeKind.EnumValue)
            {
                Value = input
            };

            var Enum = new EnumType("TestResult", new EnumValues()
            {
                ["SUCCESS"] = null,
                ["FAILURE"] = null,
                ["INCONCLUSIVE"] = null
            });

            /* When */
            var actual = Enum.ParseLiteral(astValue);

            /* Then */
            Assert.Equal(expected, actual);
        }
    }
}
using System.Linq;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
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
            var Enum = new EnumType("TestResult", new EnumValues
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
            var Enum = new EnumType("TestResult", new EnumValues
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

            var Enum = new EnumType("TestResult", new EnumValues
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

        [Fact]
        public void Define_enum()
        {
            /* Given */
            /* When */
            var Enum = new EnumType(
                "Direction",
                new EnumValues
                {
                    "NORTH",
                    "EAST",
                    "SOUTH",
                    "WEST"
                });

            /* Then */
            Assert.True(Enum.Contains("NORTH"));
            Assert.True(Enum.Contains("EAST"));
            Assert.True(Enum.Contains("SOUTH"));
            Assert.True(Enum.Contains("WEST"));
        }
    }
}
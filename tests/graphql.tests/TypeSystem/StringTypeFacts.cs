using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class StringTypeFacts
    {
        private readonly ScalarType _sut;

        public StringTypeFacts()
        {
            _sut = ScalarType.String;
        }

        [Theory]
        [InlineData("string", "string")]
        [InlineData(true, "True")]
        [InlineData(123, "123")]
        [InlineData(123.123, "123.123")]
        public void ParseValue(object input, string expected)
        {
            /* Given */
            /* When */
            var actual = _sut.ParseValue(input);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("string", "string")]
        [InlineData("true", "true")]
        [InlineData("123", "123")]
        [InlineData("123.123", "123.123")]
        public void ParseLiteral(string input, string expected)
        {
            /* Given */
            var astValue = new GraphQLScalarValue(ASTNodeKind.StringValue)
            {
                Value = input
            };

            /* When */
            var actual = _sut.ParseLiteral(astValue);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("string", "string")]
        [InlineData(true, "True")]
        [InlineData(123, "123")]
        [InlineData(123.123, "123.123")]
        public void Serialize(object input, string expected)
        {
            /* Given */
            /* When */
            var actual = _sut.Serialize(input);

            /* Then */
            Assert.Equal(expected, actual);
        }
    }
}
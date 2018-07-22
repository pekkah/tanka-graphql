using fugu.graphql.type;
using GraphQLParser.AST;
using Xunit;

namespace fugu.graphql.tests.type
{
    public class FloatTypeFacts
    {
        private readonly ScalarType _sut;

        public FloatTypeFacts()
        {
            _sut = ScalarType.Float;
        }

        [Theory]
        [InlineData(123, 123)]
        [InlineData(123.123, 123.123)]
        [InlineData("123", 123)]
        [InlineData("123.123", 123.123)]
        public void ParseValue(object input, double expected)
        {
            /* Given */
            /* When */
            var actual = _sut.ParseValue(input);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData("123.123", 123.123)]
        public void ParseLiteral(string input, double expected)
        {
            /* Given */
            var astValue = new GraphQLScalarValue(ASTNodeKind.FloatValue)
            {
                Value = input
            };

            /* When */
            var actual = _sut.ParseLiteral(astValue);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("123", 123)]
        public void ParseIntLiteral(string input, double expected)
        {
            /* Given */
            var astValue = new GraphQLScalarValue(ASTNodeKind.IntValue)
            {
                Value = input
            };

            /* When */
            var actual = _sut.ParseLiteral(astValue);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123, 123)]
        [InlineData(123.123, 123.123)]
        [InlineData("123", 123)]
        [InlineData("123.123", 123.123)]
        public void Serialize(object input, double expected)
        {
            /* Given */
            /* When */
            var actual = _sut.Serialize(input);

            /* Then */
            Assert.Equal(expected, actual);
        }
    }
}
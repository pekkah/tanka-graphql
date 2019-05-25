﻿using tanka.graphql.type;
using GraphQLParser.AST;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class IntTypeFacts
    {
        private readonly ScalarType _sut;

        public IntTypeFacts()
        {
            _sut = ScalarType.Int;
        }

        [Theory]
        [InlineData(123, 123)]
        [InlineData("123", 123)]
        public void ParseValue(object input, int expected)
        {
            /* Given */
            /* When */
            var actual = _sut.ParseValue(input);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("123", 123)]
        public void ParseLiteral(string input, int expected)
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
        [InlineData("123", 123)]
        public void Serialize(object input, int expected)
        {
            /* Given */
            /* When */
            var actual = _sut.Serialize(input);

            /* Then */
            Assert.Equal(expected, actual);
        }
    }
}
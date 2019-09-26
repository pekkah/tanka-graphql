﻿using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class BooleanTypeFacts
    {
        private readonly ScalarType _sut;

        public BooleanTypeFacts()
        {
            _sut = ScalarType.Boolean;
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public void ParseValue(object input, bool expected)
        {
            /* Given */
            /* When */
            var actual = _sut.ParseValue(input);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("True", true)]
        [InlineData("true", true)]
        [InlineData("False", false)]
        [InlineData("false", false)]
        [InlineData("1", true)]
        [InlineData("0", false)]
        public void ParseLiteral(string input, bool expected)
        {
            /* Given */
            var astValue = new GraphQLScalarValue(ASTNodeKind.BooleanValue)
            {
                Value = input
            };

            /* When */
            var actual = _sut.ParseLiteral(astValue);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory(Skip = "Coercion not allowed")]
        [InlineData("1", true)]
        [InlineData("0", false)]
        public void ParseIntLiteral(string input, bool expected)
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
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(1, true)]
        [InlineData(0, false)]
        [InlineData("1", true)]
        [InlineData("0", false)]
        public void Serialize(object input, bool expected)
        {
            /* Given */
            /* When */
            var actual = _sut.Serialize(input);

            /* Then */
            Assert.Equal(expected, actual);
        }
    }
}
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests
{
    public class FieldSetScalarConverterFacts
    {
        public FieldSetScalarConverterFacts()
        {
            Sut = new FieldSetScalarConverter();
        }

        protected FieldSetScalarConverter Sut { get; }

        [Fact]
        public void ParseLiteral()
        {
            /* Given */
            StringValue input = "one two";

            /* When */
            var actual = Sut.ParseLiteral(input)
                as string;

            /* Then */
            Assert.Equal("one two", actual);
        }

        [Fact]
        public void ParseValue()
        {
            /* Given */
            var input = "one two";

            /* When */
            var actual = Sut.ParseValue(input)
                as string;

            /* Then */
            Assert.Equal(input, actual);
        }

        [Fact]
        public void Serialize()
        {
            /* Given */
            var input = "one two";

            /* When */
            var actual = Sut.Serialize(input);

            /* Then */
            Assert.Equal(input, actual);
        }

        [Fact]
        public void SerializeLiteral()
        {
            /* Given */
            var input = "one two";

            /* When */
            var actual = Sut.SerializeLiteral(input)
                as StringValue;

            /* Then */
            Assert.Equal("one two", actual.ToString());
        }
    }
}
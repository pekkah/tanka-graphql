using System.Collections.Generic;
using Tanka.GraphQL.Generator.Integration.Tests.Model;
using Xunit;

namespace Tanka.GraphQL.Generator.Integration.Tests
{
    public class GeneratedInputObjectFacts
    {
        private readonly TestInputObject _sut;

        public GeneratedInputObjectFacts()
        {
            _sut = new TestInputObject();
        }

        [Fact]
        public void Read()
        {
            /* Given */
            var source = new Dictionary<string, object>()
            {
                ["int"] = 123,
                ["string"] = "123",
                ["float"] = 123.12,
                ["boolean"] = true
            };
            
            /* When */
            _sut.Read(source);

            /* Then */
            Assert.Equal(123, _sut.Int);
            Assert.Equal("123", _sut.String);
            Assert.Equal(123.12, _sut.Float);
            Assert.Equal(true, _sut.Boolean);
        }
    }
}
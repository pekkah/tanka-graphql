using NSubstitute;
using Xunit;

namespace Tanka.GraphQL.Tests
{
    public class ExtensionsRunnerFacts
    {
        [Fact]
        public void Get_ExtensionScope_by_type()
        {
            /* Given */
            var expected = Substitute.For<IExtensionScope>();

            var sut = new ExtensionsRunner(new []{expected});

            /* When */
            var actual = sut.Extension(expected.GetType());

            /* Then */
            Assert.Same(expected, actual);
        }
    }
}
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class TypeFacts
    {
        [Fact]
        public void FromBytes()
        {
            /* Given */
            /* When */
            TypeBase original = Encoding.UTF8.GetBytes("String")
                .AsReadOnlySpan();

            /* Then */
            Assert.IsType<NamedType>(original);
        }
        
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            TypeBase original = "String";

            /* Then */
            Assert.IsType<NamedType>(original);
        }
    }
}
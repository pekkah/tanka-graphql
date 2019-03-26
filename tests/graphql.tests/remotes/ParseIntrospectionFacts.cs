using tanka.graphql.introspection;
using Xunit;

namespace tanka.graphql.tests.remotes
{
    public class ParseIntrospectionFacts
    {
        [Fact]
        public void Parse_types()
        {
            /* Given */
            var introspection = "";

            /* When */
            var typeDefs = IntrospectionParser.ParseTo()

            /* Then */
        }
    }
}
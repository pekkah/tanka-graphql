using System.Collections.Generic;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class DirectiveInstanceFacts
    {
        [Fact]
        public void Create()
        {
            /* Given */
            var arg = "Deprecated for a good reason";
            var directiveType = DirectiveType.Deprecated;

            /* When */
            var directive = directiveType.CreateInstance(new Dictionary<string, object>()
            {
                ["reason"] = arg
            });

            /* Then */
            var reason = directive.GetArgument<string>("reason");
            Assert.Equal(arg, reason);
        }


        [Fact]
        public void Can_have_directives()
        {
            /* public static IEnumerable<DirectiveLocation> TypeSystemLocations = new[]
            {
                DirectiveLocation.SCHEMA,
                DirectiveLocation.SCALAR,
                DirectiveLocation.OBJECT,
                DirectiveLocation.FIELD_DEFINITION,
                DirectiveLocation.ARGUMENT_DEFINITION,
                DirectiveLocation.INTERFACE,
                DirectiveLocation.UNION,
                DirectiveLocation.ENUM,
                DirectiveLocation.ENUM_VALUE,
                DirectiveLocation.INPUT_OBJECT,
                DirectiveLocation.INPUT_FIELD_DEFINITION
            };*/

            var typeSystemTypes = new[]
            {
                typeof(ISchema),
                typeof(ScalarType),
                typeof(ObjectType),
                typeof(IField),
                typeof(Argument),
                typeof(InterfaceType),
                typeof(UnionType),
                typeof(EnumType),
                typeof(EnumValue),
                typeof(InputObjectType),
                typeof(InputObjectField)
            };

            foreach (var canHaveDirectives in typeSystemTypes)
                Assert.True(typeof(IHasDirectives).IsAssignableFrom(canHaveDirectives),
                    $"{canHaveDirectives} does not implement IHasDirectives");
        }
    }
}
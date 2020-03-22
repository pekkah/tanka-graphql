using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class ObjectDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            ObjectDefinition original =
                @"type Obj {
                    field1: String
                }";

            /* Then */
            Assert.Equal("Obj", original.Name);
            Assert.NotNull(original.Fields);
        }

        [Fact]
        public void Rename()
        {
            /* Given */
            ObjectDefinition original = @"type Obj";

            /* When */
            var renamed = original
                .WithName("Renamed");

            /* Then */
            Assert.Equal("Obj", original.Name);
            Assert.Equal("Renamed", renamed.Name);
        }

        [Fact]
        public void WithFields()
        {
            /* Given */
            ObjectDefinition original = @"type Obj";

            /* When */
            var withFields = original
                .WithFields(new List<FieldDefinition>
                {
                    "field: String!"
                });

            /* Then */
            Assert.Null(original.Fields);
            Assert.NotNull(withFields.Fields);
            Assert.NotEmpty(withFields.Fields);
        }

        [Fact]
        public void WithFields_Modify()
        {
            /* Given */
            ObjectDefinition original = @"type Obj { field: String }";

            /* When */
            var withFields = original
                .WithFields(original
                    .Fields?
                    .Select(originalField => originalField
                        .WithDescription("Description"))
                    .ToList()
                );

            /* Then */
            Assert.NotNull(withFields.Fields);
            var field = Assert.Single(withFields.Fields);
            Assert.Equal("Description", field?.Description);
        }
    }
}
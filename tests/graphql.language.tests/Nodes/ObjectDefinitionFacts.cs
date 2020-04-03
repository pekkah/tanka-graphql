using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class ObjectDefinitionFacts
    {
        [Fact]
        public void FromBytes()
        {
            /* Given */
            /* When */
            ObjectDefinition original =
                Encoding.UTF8.GetBytes(@"type Obj {
                    field1: String
                }").AsReadOnlySpan();

            /* Then */
            Assert.Equal("Obj", original.Name);
            Assert.NotNull(original.Fields);
        }
        
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
        public void WithDescription()
        {
            /* Given */
            ObjectDefinition original = @"type Obj";

            /* When */
            var modified = original
                .WithDescription("Description");

            /* Then */
            Assert.Null(original.Description);
            Assert.Equal("Description", modified.Description);
        }

        [Fact]
        public void WithName()
        {
            /* Given */
            ObjectDefinition original = @"type Obj";

            /* When */
            var modified = original
                .WithName("Renamed");

            /* Then */
            Assert.Equal("Obj", original.Name);
            Assert.Equal("Renamed", modified.Name);
        }

        [Fact]
        public void WithFields()
        {
            /* Given */
            ObjectDefinition original = @"type Obj";

            /* When */
            var modified = original
                .WithFields(new List<FieldDefinition>
                {
                    "field: String!"
                });

            /* Then */
            Assert.Null(original.Fields);
            Assert.NotNull(modified.Fields);
            Assert.NotEmpty(modified.Fields);
        }

        [Fact]
        public void WithFields_Modify()
        {
            /* Given */
            ObjectDefinition original = @"type Obj { field: String }";

            /* When */
            var modified = original
                .WithFields(original
                    .Fields?
                    .Select(originalField => originalField
                        .WithDescription("Description"))
                    .ToList()
                );

            /* Then */
            Assert.NotNull(modified.Fields);
            var field = Assert.Single(modified.Fields);
            Assert.Equal("Description", field?.Description);
        }

        [Fact]
        public void WithDirectives()
        {
            /* Given */
            ObjectDefinition original = @"type Obj";

            /* When */
            var modified = original
                .WithDirectives(new List<Directive>
                {
                    "@a"
                });

            /* Then */
            Assert.Null(original.Directives);
            Assert.NotNull(modified.Directives);
            var a = Assert.Single(modified.Directives);
            Assert.Equal("a", a?.Name);
        }

        [Fact]
        public void WithInterfaces()
        {
            /* Given */
            ObjectDefinition original = @"type Obj";

            /* When */
            var modified = original
                .WithInterfaces(new List<NamedType>
                {
                    "Inf1",
                    "Inf2"
                });

            /* Then */
            Assert.Null(original.Interfaces);
            Assert.NotNull(modified.Interfaces);
            Assert.Equal(2, modified.Interfaces?.Count);
        }
    }
}
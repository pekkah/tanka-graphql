using System;
using System.Linq;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class SchemaFacts
    {
        public SchemaFacts()
        {
            Schema = new SchemaBuilder()
                .InputObject("input", out var input)
                .Connections(connect => connect
                    .InputField(input, "id", ScalarType.ID))
                .Query(out var query)
                .Connections(connect => connect
                    .Field(query, "name", ScalarType.String))
                .Build();
        }

        protected readonly ISchema Schema;

        [Fact]
        public void GetDirective()
        {
            /* Given */
            var directiveTypeName = DirectiveType.Include.Name;

            /* When */
            var directiveType = Schema.GetDirective(directiveTypeName);

            /* Then */
            Assert.NotNull(directiveType);
            Assert.IsType<DirectiveType>(directiveType);
        }

        [Fact]
        public void GetField()
        {
            /* Given */
            var namedTypeName = Schema.Query.Name;

            /* When */
            var field = Schema.GetField(namedTypeName, "name");

            /* Then */
            Assert.NotNull(field);
            Assert.Same(ScalarType.String, field.Type);
        }

        [Fact]
        public void GetFields()
        {
            /* Given */
            var namedTypeName = Schema.Query.Name;

            /* When */
            var fields = Schema.GetFields(namedTypeName);

            /* Then */
            Assert.Single(
                fields,
                kv => kv.Key == "name" && (ScalarType) kv.Value.Type == ScalarType.String);
        }

        [Fact]
        public void GetInputField()
        {
            /* Given */
            var inputTypeName = "input";

            /* When */
            var field = Schema.GetInputField(inputTypeName, "id");

            /* Then */
            Assert.NotNull(field);
            Assert.Same(ScalarType.ID, field.Type);
        }

        [Fact]
        public void GetInputFields()
        {
            /* Given */
            var inputTypeName = "input";

            /* When */
            var fields = Schema.GetInputFields(inputTypeName);

            /* Then */
            Assert.Single(
                fields,
                kv => kv.Key == "id" && (ScalarType) kv.Value.Type == ScalarType.ID);
        }

        [Fact]
        public void GetNamedType()
        {
            /* Given */
            var namedTypeName = Schema.Query.Name;

            /* When */
            var namedType = Schema.GetNamedType(namedTypeName);

            /* Then */
            Assert.NotNull(namedType);
            Assert.IsAssignableFrom<INamedType>(namedType);
        }

        [Fact]
        public void QueryDirectives()
        {
            /* Given */
            bool AppliesToField(DirectiveType type)
            {
                return type.Locations.Contains(DirectiveLocation.FIELD);
            }

            /* When */
            var directives = Schema.QueryDirectives(AppliesToField);

            /* Then */
            foreach (var directiveType in directives) Assert.Contains(DirectiveLocation.FIELD, directiveType.Locations);
        }

        [Fact]
        public void QueryNamedTypes()
        {
            /* Given */
            bool TypesWithoutDescription(ObjectType type)
            {
                return string.IsNullOrEmpty(type.Description);
            }

            /* When */
            var undocumentedTypes = Schema.QueryTypes<ObjectType>(TypesWithoutDescription);

            /* Then */
            Assert.NotNull(undocumentedTypes);
            Assert.Single(undocumentedTypes, type => type.Name == "Query");
        }

        [Fact]
        public void Roots_Mutation()
        {
            /* Given */
            /* When */
            /* Then */
            Assert.Null(Schema.Mutation);
        }

        [Fact]
        public void Roots_Query()
        {
            /* Given */
            /* When */
            /* Then */
            Assert.NotNull(Schema.Query);
            Assert.IsType<ObjectType>(Schema.Query);
        }

        [Fact]
        public void Roots_Subscription()
        {
            /* Given */
            /* When */
            /* Then */
            Assert.Null(Schema.Subscription);
        }

        [Fact]
        public void Included_directives()
        {
            /* Given */ 
            /* When */
            var directives = Schema.QueryDirectives();

            /* Then */
            Assert.Single(directives, DirectiveType.Include);
            Assert.Single(directives, DirectiveType.Skip);
        }

        [Fact]
        public void Included_scalars()
        {
            /* Given */
            /* When */
            var scalars = Schema.QueryTypes<ScalarType>();

            /* Then */
            Assert.Equal(ScalarType.Standard, scalars);
        }
    }
}
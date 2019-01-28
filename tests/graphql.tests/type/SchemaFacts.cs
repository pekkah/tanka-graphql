using System;
using System.Threading.Tasks;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class SchemaFacts
    {
        [Fact]
        public void Require_Query()
        {
            /* Given */

            /* When */
            var exception = Assert.Throws<ArgumentNullException>(() => new Schema(null));

            /* Then */
            Assert.Equal("query", exception.ParamName);
        }

        [Fact]
        public void Set_Mutation()
        {
            /* Given */
            var queryType = new ObjectType(
                "Query",
                new Fields());

            var mutationType = new ObjectType(
                "Mutation",
                new Fields());

            /* When */
            var sut = new Schema(queryType, mutationType);

            /* Then */
            Assert.Equal(mutationType, sut.Mutation);
        }

        [Fact]
        public void Set_Query()
        {
            /* Given */
            var queryType = new ObjectType(
                "Query",
                new Fields());

            /* When */
            var sut = new Schema(queryType);

            /* Then */
            Assert.Equal(queryType, sut.Query);
        }

        [Fact]
        public void Set_Subscription()
        {
            /* Given */
            var queryType = new ObjectType(
                "Query",
                new Fields());

            var mutationType = new ObjectType(
                "Mutation",
                new Fields());

            var subscriptionType = new ObjectType(
                "Subscription",
                new Fields());

            /* When */
            var sut = new Schema(queryType, mutationType, subscriptionType);

            /* Then */
            Assert.Equal(subscriptionType, sut.Subscription);
        }

        [Fact]
        public async Task Initialize_types()
        {
            /* Given */
            var queryType = new ObjectType(
                "Q",
                new Fields()
                {
                    ["field"] = new Field(new ObjectType("fieldType", new Fields()))
                 });

            var sut = new Schema(queryType);

            /* When */
            await sut.InitializeAsync();

            /* Then */
            var types = sut.QueryTypes<INamedType>();

            Assert.Contains(types, t => t.Name == "Q");
            Assert.Contains(types, t => t.Name == "fieldType");
        }

        [Fact]
        public async Task Initialize_types_with_found_scalars()
        {
            /* Given */
            var queryType = new ObjectType(
                "Q",
                new Fields()
                {
                    ["field"] = new Field(new ObjectType("fieldType", new Fields()
                    {
                        ["scalar"] = new Field(ScalarType.String)
                    }))
                });

            var sut = new Schema(queryType);

            /* When */
            await sut.InitializeAsync();

            /* Then */
            var types = sut.QueryTypes<INamedType>();

            Assert.Single(types, ScalarType.String);
        }

        [Fact]
        public async Task Initialize_types_no_duplicates()
        {
            /* Given */
            var type = new ObjectType("fieldType", new Fields());
            var queryType = new ObjectType(
                "Q",
                new Fields()
                {
                    ["field"] = new Field(type),
                    ["field2"] = new Field(type)
                });

            var sut = new Schema(queryType);

            /* When */
            await sut.InitializeAsync();

            /* Then */
            var types = sut.QueryTypes<INamedType>();

            Assert.Single(types, t => t.Name == "Q");
            Assert.Single(types, t => t.Name == "fieldType");
        }

        [Fact(Skip = "Should these be included?")]
        public async Task Initialize_types_with_default_scalars()
        {
            /* Given */
            var type = new ObjectType("fieldType", new Fields());
            var queryType = new ObjectType(
                "Q",
                new Fields()
                {
                    ["field"] = new Field(type),
                    ["field2"] = new Field(type)
                });

            var sut = new Schema(queryType);

            /* When */
            await sut.InitializeAsync();

            /* Then */
            var types = sut.QueryTypes<INamedType>();

            foreach (var scalar in ScalarType.Standard)
            {
                Assert.Single(types, scalar);
            }
        }

        [Fact]
        public async Task Initialize_directives_with_skip_and_include()
        {
            /* Given */
            var type = new ObjectType("fieldType", new Fields());
            var queryType = new ObjectType(
                "Q",
                new Fields()
                {
                    ["field"] = new Field(type),
                    ["field2"] = new Field(type)
                });

            var sut = new Schema(queryType);

            /* When */
            await sut.InitializeAsync();

            /* Then */
            var directives = sut.QueryDirectives();

            Assert.Single(directives, DirectiveType.Skip);
            Assert.Single(directives, DirectiveType.Include);
        }

        [Fact]
        public async Task Initialize_heal_schema()
        {
            /* Given */
            var type = new ObjectType("fieldType", new Fields());
            var typeReference = new NamedTypeReference("fieldType");
            var field = new Field(typeReference);
            var queryType = new ObjectType(
                "Q",
                new Fields()
                {
                    ["field"] = field,
                });

            var sut = new Schema(queryType, typesReferencedByNameOnly: new []{ type });

            /* When */
            await sut.InitializeAsync();

            /* Then */
            var actual = sut.Query.GetField("field");
            Assert.Equal(type, actual.Type);

        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests
{
    public class FederationSchemaBuilderFacts
    {
        [Fact]
        public void EntityUnion_does_not_contain_object_without_key_directive()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .AddFederationDirectives()
                .Sdl(@"
                    type Person @key(fields: ""id"") {
                        id: ID!
                    }
                    type Address {
                        street: String
                    }")
                .Query(out var query)
                .AddFederationSchemaExtensions(new DictionaryReferenceResolversMap());

            /* When */
            var schema = builder.Build();
            var entityUnion = schema.GetNamedType<UnionType>("_Entity");
            var entities = schema.GetPossibleTypes(entityUnion)
                .ToList();

            /* Then */
            Assert.Single(entities);
        }

        [Fact]
        public void EntityUnion_has_possible_type_with_key_directive()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .AddFederationDirectives()
                .Sdl(@"
                    type Person @key(fields: ""id"") {
                        id: ID!
                    }")
                .Query(out var query)
                .AddFederationSchemaExtensions(new DictionaryReferenceResolversMap());

            /* When */
            var schema = builder.Build();
            var entityUnion = schema.GetNamedType<UnionType>("_Entity");
            var entities = schema.GetPossibleTypes(entityUnion);

            /* Then */
            Assert.Single(entities, obj => obj.Name == "Person");
        }

        [Fact]
        public async Task Query_entities()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .AddFederationDirectives()
                .Sdl(@"
                    type Person @key(fields: ""id"") {
                        id: ID!
                        name: String!
                    }
                    type Address @key(fields: ""street"") {
                        street: String
                    }")
                .UseResolversAndSubscribers(new ObjectTypeMap
                {
                    ["Person"] = new FieldResolversMap
                    {
                        {"id", context => ResolveSync.As("ID123")},
                        {"name", context => ResolveSync.As("Name 123")}
                    }
                })
                .Query(out _)
                .AddFederationSchemaExtensions(new DictionaryReferenceResolversMap
                {
                    ["Person"] = (context, type, representation) => new ValueTask<ResolveReferenceResult>(
                        new ResolveReferenceResult(type, representation))
                });

            /* When */
            var schema = builder.Build();
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = schema,
                Document = @"query Entities($reps: [_Any!]!) { 
_entities(representations: $reps) { 
    ... on Person { 
        id 
        } 
    } 
}",
                VariableValues = new Dictionary<string, object>
                {
                    ["reps"] = new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["id"] = 101,
                            ["__typename"] = "Person"
                        }
                    }
                }
            });

            /* Then */
            Assert.Null(result.Errors);
        }

        [Fact]
        public async Task Query_sdl()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .AddFederationDirectives()
                .Sdl(@"
type Review  @key(fields: ""id"") {
  id: ID!
  product: Product
}


type Product @key(fields: ""upc"") @extends {
  upc: String! @external
}")
                .UseResolversAndSubscribers(new ObjectTypeMap())
                .AddFederationSchemaExtensions(new DictionaryReferenceResolversMap());

            /* When */
            var schema = builder.Build();
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = schema,
                Document = @"
{
    _service {
        sdl
    }
}"
            });

            /* Then */
            Assert.Null(result.Errors);
        }
    }

    public class Review
    {
        public string Upc { get; set; }
    }

    public class Product
    {
        public string Upc { get; set; }
    }
}
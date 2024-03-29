﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public class FederationSchemaBuilderFacts
{
    [Fact]
    public async Task EntityUnion_does_not_contain_object_without_key_directive()
    {
        /* Given */
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                    type Person @key(fields: ""id"") {
                        id: ID!
                    }
                    type Address {
                        street: String
                    }")
            .AddSubgraph(SubgraphOptions.Default);

        /* When */
        var schema = await builder.Build();

        var entityUnion = schema.GetRequiredNamedType<UnionDefinition>("_Entity");
        var entities = schema.GetPossibleTypes(entityUnion)
            .ToList();

        /* Then */
        Assert.Single(entities);
    }

    [Fact]
    public async Task EntityUnion_has_possible_type_with_key_directive()
    {
        /* Given */
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                    type Person @key(fields: ""id"") {
                        id: ID!
                    }")
            .AddSubgraph(SubgraphOptions.Default);

        /* When */
        var schema = await builder.Build();

        var entityUnion = schema.GetRequiredNamedType<UnionDefinition>("_Entity");
        var entities = schema.GetPossibleTypes(entityUnion);

        /* Then */
        Assert.Single(entities, obj => obj.Name == "Person");
    }

    [Fact]
    public async Task Query_entities()
    {
        /* Given */
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
                    type Person @key(fields: ""id"") {
                        id: ID!
                        name: String!
                    }
                    type Address @key(fields: ""street"") {
                        street: String
                    }")
            .AddSubgraph(new(new DictionaryReferenceResolversMap
            {
                ["Person"] = (context, type, representation) => new(
                    new ResolveReferenceResult(type, representation))
            }))
            .Add(new ResolversMap
            {
                ["Person"] = new()
                {
                    { "id", context => context.ResolveAs("ID123") },
                    { "name", context => context.ResolveAs("Name 123") }
                }
            });


        /* When */
        var schema = await builder.Build();

        var result = await new Executor(schema).Execute(new GraphQLRequest
        {
            Query = """
                query Entities($reps: [_Any!]!) { 
                _entities(representations: $reps) { 
                    ... on Person { 
                        id 
                        } 
                    } 
                }
                """,
            Variables = new Dictionary<string, object>
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
        result.ShouldMatchJson(@"{
  ""data"": {
    ""_entities"": [
      {
        ""id"": ""ID123""
      }
    ]
  },
  ""extensions"": null,
  ""errors"": null
}");
    }

    [Fact]
    public async Task Query_sdl()
    {
        /* Given */
        var builder = new ExecutableSchemaBuilder()
            .Add(@"
type Review  @key(fields: ""id"") {
  id: ID!
  product: Product
}


type Product @key(fields: ""upc"") @extends {
  upc: String! @external
}")
            .AddSubgraph(SubgraphOptions.Default);
        /* When */
        var schema = await builder.Build();

        var result = await new Executor(schema).Execute(new GraphQLRequest
        {
            Query = """
                {
                    _service {
                        sdl
                    }
                }
                """
        });

        /* Then */
        //todo: when buit in types are ignored fix this to validated the actual result
        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.links;
using tanka.graphql.resolvers;
using tanka.graphql.schema;
using tanka.graphql.sdl;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.tools
{
    public class MakeRemoteExecutableFacts
    {
        [Fact]
        public async Task Execute_with_StaticLink()
        {
            /* Given */
            var schemaOneBuilder = new SchemaBuilder()
                .Sdl(
                    @"
                    type User {
                        id: ID!
                        name: String!
                    }

                    type Query {
                        userById(id: ID!): User
                    }

                    schema {
                        query: Query
                    }
                    ");

            var schemaTwoBuilder = new SchemaBuilder()
                .Sdl(
                    @"
                    type Address {
                        city: String!
                    }

                    type User {
                        address: Address!
                    }

                    type Query {

                    }
                    "
                );

            var schemaOne = RemoteSchemaTools.MakeRemoteExecutable(
                schemaOneBuilder,
                RemoteLinks.Static(new ExecutionResult
                {
                    Data = new Dictionary<string, object>
                    {
                        ["userById"] = new Dictionary<string, object>
                        {
                            ["id"] = "1",
                            ["name"] = "name"
                        }
                    }
                }));

            var schemaTwo = SchemaTools.MakeExecutableSchema(
                schemaTwoBuilder,
                new TypeMap
                {
                    ["Address"] = new FieldResolversMap
                    {
                        {"city", context => ResolveSync.As(context.ObjectValue)}
                    },
                    ["User"] = new FieldResolversMap
                    {
                        {"address", context => ResolveSync.As("Vantaa")}
                    }
                });

            var schema = new SchemaBuilder()
                .Merge(schemaOne, schemaTwo)
                .Build();

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = schema,
                Document = Parser.ParseDocument(@"
                {
                    userById(id: ""1"") {
                        id
                        name
                        address {
                            city
                        }
                    }
                }")
            });

            /* Then */
            result.ShouldMatchJson(
                @"
                {
                  ""data"": {
                    ""userById"": {
                      ""address"": {
                        ""city"": ""Vantaa""
                      },
                      ""name"": ""name"",
                      ""id"": ""1""
                    }
                  }
                }
                ");
        }

        [Fact(Skip = "Test is flaky. Starts failing randomly.")]
        public async Task Subscriptions()
        {
            /* Given */
            var schemaOneBuilder = new SchemaBuilder()
                .Sdl(
                    @"
                    type User {
                        id: ID!
                        name: String!
                    }

                    type Query {
                        userById(id: ID!): User
                    }

                    type Subscription {
                        userAdded: User
                    }

                    schema {
                        query: Query
                        subscription: Subscription
                    }
                    ");

            var schemaTwoBuilder = new SchemaBuilder()
                .Sdl(
                    @"
                    type Address {
                        city: String!
                    }

                    type User {
                        address: Address!
                    }

                    type Query {

                    }

                    type Subscription {

                    }
                    "
                );

            var schemaOne = RemoteSchemaTools.MakeRemoteExecutable(
                schemaOneBuilder,
                RemoteLinks.Static(new ExecutionResult
                {
                    Data = new Dictionary<string, object>
                    {
                        ["userAdded"] = new Dictionary<string, object>
                        {
                            ["id"] = "1",
                            ["name"] = "name"
                        }
                    }
                }));

            var schemaTwo = SchemaTools.MakeExecutableSchema(
                schemaTwoBuilder,
                new TypeMap
                {
                    ["Address"] = new FieldResolversMap
                    {
                        {"city", context => ResolveSync.As(context.ObjectValue)}
                    },
                    ["User"] = new FieldResolversMap
                    {
                        {"address", context => ResolveSync.As("Vantaa")}
                    }
                });

            var schema = new SchemaBuilder()
                .Merge(schemaOne, schemaTwo)
                .Build();

            var unsubscribe = new CancellationTokenSource(TimeSpan.FromSeconds(30));


            /* When */
            var subscriptionResult = await Executor.SubscribeAsync(new ExecutionOptions
            {
                Schema = schema,
                Document = Parser.ParseDocument(@"
                subscription {
                    userAdded {
                        id
                        name
                        address {
                            city
                        }
                    }
                }")
            }, unsubscribe.Token);

            var result = await subscriptionResult.Source.Reader.ReadAsync(unsubscribe.Token);

            /* Then */
            result.ShouldMatchJson(
                @"
                {
                  ""data"": {
                    ""userAdded"": {
                      ""address"": {
                        ""city"": ""Vantaa""
                      },
                      ""name"": ""name"",
                      ""id"": ""1""
                    }
                  }
                }
                ");
        }
    }
}
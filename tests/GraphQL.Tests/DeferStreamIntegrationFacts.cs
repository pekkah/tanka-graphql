using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests;

public class DeferStreamIntegrationFacts
{
    [Fact]
    public async Task Defer_directive_should_work_in_inline_fragment()
    {
        // Given - Schema with user and profile data including @defer/@stream directives
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
            .Add("Query", new()
            {
                { "user: User", b => b.ResolveAs(new UserModel { id = "1", name = "John Doe" }) }
            })
            .Add("User", new()
            {
                { "id: ID!", b => b.ResolveAsPropertyOf<UserModel>(u => u.id) },
                { "name: String!", b => b.ResolveAsPropertyOf<UserModel>(u => u.name) },
                { "profile: Profile", b => b.ResolveAs(new ProfileModel { email = "john@example.com", bio = "Software developer" }) }
            })
            .Add("Profile", new()
            {
                { "email: String", b => b.ResolveAsPropertyOf<ProfileModel>(p => p.email) },
                { "bio: String", b => b.ResolveAsPropertyOf<ProfileModel>(p => p.bio) }
            })
            .Build();

        // Configure services with our extensible directive system
        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices(); // Add default services (includes IFieldCollector, skip/include)
        services.AddIncrementalDeliveryDirectives(); // Add @defer and @stream

        var serviceProvider = services.BuildServiceProvider();

        // GraphQL query with @defer directive
        ExecutableDocument query = """
            {
                user {
                    id
                    name
                    ... @defer(label: "profile") {
                        profile {
                            email
                            bio
                        }
                    }
                }
            }
            """;

        // When - Execute the query as a streaming operation
        var executorOptions = new ExecutorOptions
        {
            Schema = schema,
            ServiceProvider = serviceProvider
        };
        var executor = new Executor(executorOptions);
        var queryContext = executor.BuildQueryContextAsync(new GraphQLRequest { Query = query });
        var stream = executor.ExecuteOperation(queryContext);

        // Then - Verify incremental delivery results
        await stream.ShouldMatchStreamJson("""
                                           {
                                               "results": [
                                                   {
                                                       "data": {
                                                           "user": {
                                                               "id": "1",
                                                               "name": "John Doe"
                                                           }
                                                       },
                                                       "hasNext": true
                                                   },
                                                   {
                                                       "incremental": [
                                                           {
                                                               "label": "profile",
                                                               "path": ["user"],
                                                               "data": {
                                                                   "profile": {
                                                                       "email": "john@example.com",
                                                                       "bio": "Software developer"
                                                                   }
                                                               }
                                                           }
                                                       ],
                                                       "hasNext": false
                                                   }
                                               ]
                                           }
                                           """);
    }

    [Fact]
    public async Task Defer_directive_should_stream_incremental_results()
    {
        // Given - Schema with @defer directive
        var schema = await new ExecutableSchemaBuilder()
            .AddDeferDirective()
            .Add("Query", new()
            {
                { "user: User", b => b.ResolveAs(new UserModel { id = "1", name = "John Doe" }) }
            })
            .Add("User", new()
            {
                { "id: ID!", b => b.ResolveAsPropertyOf<UserModel>(u => u.id) },
                { "name: String!", b => b.ResolveAsPropertyOf<UserModel>(u => u.name) },
                { "profile: Profile", async context =>
                {
                    await Task.Delay(100); // Simulate async work
                    context.ResolvedValue = new ProfileModel { email = "john@example.com", bio = "Software developer" };
                }}
            })
            .Add("Profile", new()
            {
                { "email: String", b => b.ResolveAsPropertyOf<ProfileModel>(p => p.email) },
                { "bio: String", b => b.ResolveAsPropertyOf<ProfileModel>(p => p.bio) }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        services.AddSingleton<IFieldCollector, DefaultFieldCollector>();
        services.AddDeferDirective(); // Only need @defer for this test
        var serviceProvider = services.BuildServiceProvider();

        ExecutableDocument query = """
            {
                user {
                    id
                    name
                    ... @defer(label: "profile") {
                        profile {
                            email
                            bio
                        }
                    }
                }
            }
            """;

        // When - Execute as streaming operation
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var stream = executor.ExecuteOperation(new QueryContext
        {
            Request = new GraphQLRequest { Query = query }
        });

        // Then - Verify streaming results
        await stream.ShouldMatchStreamJson("""
            {
                "results": [
                    {
                        "data": {
                            "user": {
                                "id": "1",
                                "name": "John Doe"
                            }
                        },
                        "hasNext": true
                    },
                    {
                        "incremental": [
                            {
                                "label": "profile",
                                "path": ["user"],
                                "data": {
                                    "profile": {
                                        "email": "john@example.com",
                                        "bio": "Software developer"
                                    }
                                }
                            }
                        ],
                        "hasNext": false
                    }
                ]
            }
            """);
    }

    // Test models
    public record UserModel
    {
        public string id { get; init; } = "";
        public string name { get; init; } = "";
    }

    public record ProfileModel
    {
        public string email { get; init; } = "";
        public string bio { get; init; } = "";
    }
}
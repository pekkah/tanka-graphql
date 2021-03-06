﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Tutorials.GettingStarted
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // We will use memory cache to manage the cached
            // schema instance
            services.AddMemoryCache();

            // This will manage the schema
            services.AddSingleton<SchemaCache>();

            AddTanka(services);

            AddSignalRServer(services);

            AddWebSocketsServer(services);

            AddExecutionScopedService(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            UseSignalRServer(app);

            UseWebSocketsServer(app);
        }

        private void AddExecutionScopedService(IServiceCollection services)
        {
            services.AddScoped<ResolverController>();
        }

        private void AddWebSocketsServer(IServiceCollection services)
        {
            // Configure websockets
            services.AddWebSockets(options => { options.AllowedOrigins.Add("https://localhost:5000"); });

            // Add Tanka GraphQL-WS server
            services.AddTankaGraphQL()
                .ConfigureWebSockets();
        }

        private void UseWebSocketsServer(IApplicationBuilder app)
        {
            // Add Websockets middleware
            app.UseWebSockets();

            // Add Tanka GraphQL-WS middleware
            app.UseEndpoints(endpoints => endpoints.MapTankaGraphQLWebSockets("/graphql/ws"));
        }

        private static void UseSignalRServer(IApplicationBuilder app)
        {
            // add SignalR
            app.UseEndpoints(routes => { routes.MapTankaGraphQLSignalR("/graphql/hub"); });
        }

        private static void AddTanka(IServiceCollection services)
        {
            // Configure schema options
            services.AddTankaGraphQL()
                .ConfigureSchema<SchemaCache>(async cache => await cache.GetOrAdd());
        }

        private static void AddSignalRServer(IServiceCollection services)
        {
            // Configure Tanka server
            services.AddSignalR()
                .AddTankaGraphQL();
        }
    }

    public class ResolverController
    {
        public ValueTask<IResolverResult> QueryLastName()
        {
            return ResolveSync.As("GraphQL");
        }
    }

    public class SchemaCache
    {
        private readonly IMemoryCache _cache;

        public SchemaCache(
            IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<ISchema> GetOrAdd()
        {
            return _cache.GetOrCreateAsync(
                "Schema",
                entry => Create());
        }

        private async Task<ISchema> Create()
        {
            // Do some async work to build the schema. For example
            // load SDL from file
            await Task.Delay(0);

            // Build simple schema from SDL string
            var builder = new SchemaBuilder()
                .Sdl(
                    @"
                    type Query {
                        firstName: String!
                        lastName: String!
                    }

                    schema {
                        query: Query
                    }
                    ");

            // Bind resolvers and build
            return SchemaTools
                .MakeExecutableSchemaWithIntrospection(
                    builder,
                    new ObjectTypeMap
                    {
                        ["Query"] = new FieldResolversMap
                        {
                            {"firstName", context => ResolveSync.As("Tanka")},
                            {"lastName", UseService()}
                        }
                    });
        }

        private Resolver UseService()
        {
            return context => context
                .Use<ResolverController>()
                .QueryLastName();
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Tutorials.GettingStarted
{
    public class GettingStartedServer
    {
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // We will use memory cache to manage the cached
            // schema instance
            services.AddMemoryCache();

            // This will manage the schema
            services.AddSingleton<SchemaCache>();

            AddSchemaOptions(services);

            AddSignalRServer(services);

            AddWebSocketsServer(services);

            AddContextExtension(services);
        }

        private void AddContextExtension(IServiceCollection services)
        {
            services.AddScoped<ResolverController>();
            services.AddTankaServerExecutionContextExtension<ResolverController>();
        }

        private void AddWebSocketsServer(IServiceCollection services)
        {
            // Configure websockets
            services.AddWebSockets(options =>
            {
                options.AllowedOrigins.Add("https://localhost:5000");
            });

            // Add Tanka GraphQL-WS server
            services.AddTankaWebSocketServer();
        }

        public void Configure(IApplicationBuilder app)
        {
            UseSignalRServer(app);

            UseWebSocketsServer(app);
        }

        private void UseWebSocketsServer(IApplicationBuilder app)
        {
            // Add Websockets middleware
            app.UseWebSockets();

            // Add Tanka GraphQL-WS middleware
            app.UseTankaWebSocketServer(new WebSocketServerOptions()
            {
                Path = "/graphql/ws"
            });
        }

        private static void UseSignalRServer(IApplicationBuilder app)
        {
            // add SignalR
            app.UseEndpoints(routes => { routes.MapTankaServerHub("/graphql/hub"); });
        }

        private static void AddSchemaOptions(IServiceCollection services)
        {
            // Configure schema options
            services.AddTankaSchemaOptions()
                .Configure<SchemaCache>((options, cache) =>
                {
                    // executor will call get schema every request
                    options.GetSchema = async query => await cache.GetOrAdd(query);
                });
        }

        private static void AddSignalRServer(IServiceCollection services)
        {
            // Configure SignalR server
            services.AddSignalR()
                // Add SignalR server hub
                .AddTankaServerHub();
        }
    }

    public class ResolverController
    {
        public ValueTask<IResolverResult> QueryLastName(IResolverContext context)
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

        public Task<ISchema> GetOrAdd(Query query)
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
                    new ObjectTypeMap()
                    {
                        ["Query"] = new FieldResolversMap()
                        {
                            {"firstName", context => ResolveSync.As("Tanka")},
                            {"lastName", UseContextExtension()}
                        }
                    });
        }

        private Resolver UseContextExtension()
        {
            return context => context
                .Use<ResolverController>()
                .QueryLastName(context);
        }
    }
}
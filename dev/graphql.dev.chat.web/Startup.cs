using GraphQL.Server.Ui.Playground;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tanka.graphql.samples.chat.data;
using tanka.graphql.samples.chat.web.GraphQL;
using tanka.graphql.server;
using tanka.graphql.server.webSockets;

namespace tanka.graphql.samples.chat.web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // graphql
            services.AddSingleton<IChat, Chat>();
            services.AddSingleton<IChatResolverService, ChatResolverService>();
            services.AddSingleton<ChatSchemas>();
            services.AddSingleton(provider => provider.GetRequiredService<ChatSchemas>().Chat);

            // signalr server
            services.AddSignalR(options => options.EnableDetailedErrors = true)
                // add GraphQL query streaming hub
                .AddTankaServerHubWithTracing();

            // graphql-ws websocket server
            // web socket server
            services.AddTankaWebSocketServerWithTracing();

            // CORS is required for the graphql.samples.chat.ui React App
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000");
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowCredentials();
                    policy.WithHeaders("X-Requested-With", "authorization");
                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseCors();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseWebSockets();

            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions()
            {
                GraphQLEndPoint = "/api/graphql",
                Path = "/ui"
            });

            // signalr server
            app.UseSignalR(routes => { routes.MapHub<QueryStreamHub>(new PathString("/graphql")); });

            // websockets server
            app.UseTankaWebSocketServer(new WebSocketServerOptions()
            {
                Path = "/api/graphql"
            });

            app.UseMvc();
        }
    }
}
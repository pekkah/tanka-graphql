using fugu.graphql.samples.chat.data;
using fugu.graphql.samples.chat.web.GraphQL;
using fugu.graphql.server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace fugu.graphql.samples.chat.web
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
            services.AddSingleton<ServerClients>();
            services.AddSignalR(options => { options.EnableDetailedErrors = true; });
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000");
                    policy.AllowAnyHeader();
                    policy.WithHeaders("X-Requested-With");
                    policy.AllowAnyMethod();
                    policy.AllowCredentials();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            app.UseSignalR(routes => { routes.MapHub<ServerHub>(new PathString("/graphql-ws")); });
        }
    }
}
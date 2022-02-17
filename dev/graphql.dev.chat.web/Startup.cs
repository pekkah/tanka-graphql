using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tanka.GraphQL.Extensions.Analysis;
using Tanka.GraphQL.Extensions.Tracing;
using Tanka.GraphQL.Samples.Chat.Data;
using Tanka.GraphQL.Samples.Chat.Web.GraphQL;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Server.Links.DTOs;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Samples.Chat.Web;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        Env = env;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Env { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // required to serialize 
                options.JsonSerializerOptions.Converters
                    .Add(new ObjectDictionaryConverter());
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

        // graphql
        services.AddSingleton<IChat, Data.Chat>();
        services.AddSingleton<IChatResolverService, ChatResolverService>();
        services.AddSingleton<ChatSchemas>();
        services.AddSingleton(provider => provider.GetRequiredService<ChatSchemas>().Chat);

        // configure execution options
        var tanka = services.AddTankaGraphQL()
            .ConfigureRules(rules => rules.Concat(new[]
            {
                CostAnalyzer.MaxCost(100, 1, true)
            }).ToArray())
            .ConfigureSchema<ISchema>(schema => new ValueTask<ISchema>(schema))
            .ConfigureWebSockets();

        //if (Env.IsDevelopment()) tanka.AddExtension<TraceExtension>();

        // signalr server
        services.AddSignalR(options => options.EnableDetailedErrors = true)
            .AddTankaGraphQL();

        // graphql-ws websocket server
        // web socket server
        services.AddWebSockets(options =>
        {
            options.AllowedOrigins.Add("http://localhost:5000");
            options.AllowedOrigins.Add("http://localhost:3000");
        });

        // CORS is required for the graphql.samples.chat.ui React App
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:3000");
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowCredentials();
                //policy.WithHeaders("X-Requested-With", "authorization");
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapTankaGraphQLSignalR("/graphql");
            //endpoints.MapTankaGraphQLWebSockets("/api/graphql");

            endpoints.MapControllers();
        });
    }
}
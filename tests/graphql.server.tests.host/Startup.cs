using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql;
using fugu.graphql.resolvers;
using fugu.graphql.server;
using fugu.graphql.tools;
using fugu.graphql.type;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace graphql.server.tests.host
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var eventManager = new EventManager();
            var query = new ObjectType("Query", new Fields
            {
                {"hello", new Field(ScalarType.String)}
            });

            var sub = new ObjectType("Subscription", new Fields
            {
                {"helloEvents", new Field(ScalarType.String)}
            });

            var resolvers = new ResolverMap
            {
                {
                    query.Name, new FieldResolverMap
                    {
                        {"hello", context => Task.FromResult(Resolve.As("world"))}
                    }
                },
                {
                    sub.Name, new FieldResolverMap
                    {
                        {
                            "helloEvents", context => Task.FromResult(Resolve.Stream(
                                eventManager.HelloEvents,
                                () => Task.CompletedTask)),
                            context => Task.FromResult(Resolve.As(context.ObjectValue))
                        }
                    }
                }
            };

            var executable = SchemaTools
                .MakeExecutableSchemaWithIntrospection(new Schema(query, null, sub), resolvers, resolvers).Result;
            services.AddSingleton(provider => executable);
            services.AddSingleton(provider => eventManager);
            services.AddSingleton<ServerClients>();

            services.AddSignalR(options => { options.EnableDetailedErrors = true; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseSignalR(routes => { routes.MapHub<ServerHub>(new PathString("/graphql")); });
        }
    }

    public class EventManager
    {
        private readonly BufferBlock<string> _buffer;

        public EventManager()
        {
            _buffer = new BufferBlock<string>();
        }

        public ISourceBlock<string> HelloEvents => _buffer;

        public Task Hello(string message)
        {
            return _buffer.SendAsync(message);
        }
    }
}
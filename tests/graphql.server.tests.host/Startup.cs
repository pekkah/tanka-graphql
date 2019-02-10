using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using tanka.graphql;
using tanka.graphql.resolvers;
using tanka.graphql.server;
using tanka.graphql.tools;
using tanka.graphql.type;
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
                            "helloEvents", (context,ct) =>
                            {
                                var events = eventManager.Subscribe(ct);
                                return Task.FromResult(Resolve.Stream(events));
                            },
                            context => Task.FromResult(Resolve.As(context.ObjectValue))
                        }
                    }
                }
            };

            var executable = SchemaTools
                .MakeExecutableSchemaWithIntrospection(Schema.Initialize(query, null, sub), resolvers, resolvers).Result;
            services.AddSingleton(provider => executable);
            services.AddSingleton(provider => eventManager);

            services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                .AddQueryStreamHubWithTracing();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseSignalR(routes =>
            {
                routes.MapHub<QueryStreamHub>(new PathString("/graphql"));
            });
        }
    }

    public class EventManager
    {
        private readonly BufferBlock<string> _buffer;

        public EventManager()
        {
            _buffer = new BufferBlock<string>();
        }

        public Task Hello(string message)
        {
            return _buffer.SendAsync(message);
        }

        public ISourceBlock<string> Subscribe(CancellationToken cancellationToken)
        {
            var targetBlock = new BufferBlock<string>();

            var sub = _buffer.LinkTo(targetBlock);
            cancellationToken.Register(() =>
            {
                sub.Dispose();
            });

            return targetBlock;
        }
    }
}
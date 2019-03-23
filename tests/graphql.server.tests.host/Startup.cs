using System.Threading;
using System.Threading.Tasks;
using tanka.graphql;
using tanka.graphql.resolvers;
using tanka.graphql.server;
using tanka.graphql.tools;
using tanka.graphql.type;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using tanka.graphql.channels;
using tanka.graphql.sdl;

namespace graphql.server.tests.host
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var eventManager = new EventManager();
            var sdl = @"
                input InputEvent {
                    type: String!
                    message: String!
                }

                type Event {
                    type: String!
                    message: String!
                }

                type Query {
                    hello: String!
                }

                type Mutation {
                    add(event: InputEvent!): Event
                }

                type Subscription {
                    events: Event!
                }

                schema {
                    query: Query
                    mutation: Mutation
                }
                ";

            var builder = new SchemaBuilder();
            Sdl.Import(Parser.ParseDocument(sdl), builder);

            var resolvers = new ResolverMap
            {
                {
                    "Event", new FieldResolverMap()
                    {
                        {"type", Resolve.PropertyOf<Event>(ev => ev.Type)},
                        {"message", Resolve.PropertyOf<Event>(ev => ev.Message)}
                    }
                },
                {
                    "Query", new FieldResolverMap
                    {
                        {"hello", context => new ValueTask<IResolveResult>(Resolve.As("world"))}
                    }
                },
                {
                    "Mutation", new FieldResolverMap()
                    {
                        {"add", async context =>
                            {
                                var input = context.GetArgument<InputEvent>("event");
                                var ev = await eventManager.Add(input.Type, input.Message);

                                return Resolve.As(ev);
                            }
                        }
                    }
                },
                {
                    "Subscription", new FieldResolverMap
                    {
                        {
                            "events", (context,ct) =>
                            {
                                var events = eventManager.Subscribe(ct);
                                return new ValueTask<ISubscribeResult>(events);
                            },
                            context => new ValueTask<IResolveResult>(Resolve.As(context.ObjectValue))
                        }
                    }
                }
            };

            var executable = SchemaTools.MakeExecutableSchemaWithIntrospection(
                builder, 
                resolvers, 
                resolvers);

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

    public class Event
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public class InputEvent
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public class EventManager
    {
        private readonly PoliteEventChannel<Event> _channel;

        public EventManager()
        {
            _channel = new PoliteEventChannel<Event>(new Event()
            {
                Type = "welcome",
                Message = "Welcome"
            });
        }

        public async Task<Event> Add(string type, string message)
        {
            var ev = new Event()
            {
                Type = type,
                Message = message
            };
            await _channel.WriteAsync(ev);
            return ev;
        }

        public ISubscribeResult Subscribe(CancellationToken cancellationToken)
        {
            return _channel.Subscribe(cancellationToken);
        }
    }
}
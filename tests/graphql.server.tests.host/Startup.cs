using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server.Tests.Host
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

            var builder = new SchemaBuilder()
                .Sdl(Parser.ParseTypeSystemDocument(sdl));

            var resolvers = new ObjectTypeMap
            {
                {
                    "Event", new FieldResolversMap
                    {
                        {"type", Resolve.PropertyOf<Event>(ev => ev.Type)},
                        {"message", Resolve.PropertyOf<Event>(ev => ev.Message)}
                    }
                },
                {
                    "Query", new FieldResolversMap
                    {
                        {"hello", context => new ValueTask<IResolverResult>(Resolve.As("world"))}
                    }
                },
                {
                    "Mutation", new FieldResolversMap
                    {
                        {
                            "add", async context =>
                            {
                                var input = context.GetObjectArgument<InputEvent>("event");
                                var ev = await eventManager.Add(input.Type, input.Message);

                                return Resolve.As(ev);
                            }
                        }
                    }
                },
                {
                    "Subscription", new FieldResolversMap
                    {
                        {
                            "events", (context, ct) =>
                            {
                                var events = eventManager.Subscribe(ct);
                                return new ValueTask<ISubscriberResult>(events);
                            },
                            context => new ValueTask<IResolverResult>(Resolve.As(context.ObjectValue))
                        }
                    }
                }
            };

            var executable = SchemaTools.MakeExecutableSchemaWithIntrospection(
                builder,
                resolvers,
                resolvers);

            services.AddSingleton(provider => eventManager);

            // configure common options and add web socket services
            services.AddTankaGraphQL()
                .ConfigureSchema(()=> new ValueTask<ISchema>(executable))
                .ConfigureWebSockets();

            // add SignalR services and Tanka SignalR hub services
            services.AddSignalR()
                .AddTankaGraphQL();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseWebSockets();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapTankaGraphQLSignalR("/graphql");
                endpoints.MapTankaGraphQLWebSockets("/api/graphql");
            });
        }
    }

    public class Event
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public class InputEvent : IReadFromObjectDictionary
    {
        public string Type { get; set; }
        public string Message { get; set; }

        public void Read(IReadOnlyDictionary<string, object> source)
        {
            Type = source.GetValue<string>("type");
            Message = source.GetValue<string>("message");
        }
    }

    public class EventManager
    {
        private readonly PoliteEventChannel<Event> _channel;

        public EventManager()
        {
            _channel = new PoliteEventChannel<Event>(new Event
            {
                Type = "welcome",
                Message = "Welcome"
            });
        }

        public async Task<Event> Add(string type, string message)
        {
            var ev = new Event
            {
                Type = type,
                Message = message
            };
            await _channel.WriteAsync(ev);
            return ev;
        }

        public ISubscriberResult Subscribe(CancellationToken cancellationToken)
        {
            return _channel.Subscribe(cancellationToken);
        }
    }
}
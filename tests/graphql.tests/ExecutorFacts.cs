using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.schema;
using tanka.graphql.sdl;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;
using Xunit.Abstractions;

namespace tanka.graphql.tests
{
    public class ExecutorFacts
    {
        public const string Sdl = @"
                enum EventType {
                    INSERT
                    UPDATE
                    DELETE
                }

                type Success {
                    id: ID!
                    event: Event
                }

                type Failure {
                    message: String!
                }

                union Result = Success | Failure

                type Event {
                    id: ID!
                    type: EventType!
                    payload: String
                }

                input NewEvent {
                    type: String!
                    payload: String
                }

                type Query {
                    events: [Event!]
                }

                type Mutation {
                    create(event: NewEvent!): Result
                }

                type Subscription {
                    events: Event!
                }

                schema {
                    query: Query
                    mutation: Mutation
                    subscription: Subscription
                }
                ";

        public ExecutorFacts(ITestOutputHelper atr)
        {
            Model = new EventsModel();
            Resolvers = new TypeMap
            {
                {
                    "Success", new FieldResolversMap
                    {
                        {"id", Resolve.PropertyOf<EventsModel.Success>(m => m.Id)},
                        {"event", Resolve.PropertyOf<EventsModel.Success>(m => m.Event)}
                    }
                },
                {
                    "Failure", new FieldResolversMap
                    {
                        {"message", Resolve.PropertyOf<EventsModel.Failure>(m => m.Message)}
                    }
                },
                {
                    "Event", new FieldResolversMap
                    {
                        {"id", Resolve.PropertyOf<EventsModel.Event>(ev => ev.Id)},
                        {"type", Resolve.PropertyOf<EventsModel.Event>(ev => ev.Type)},
                        {"payload", Resolve.PropertyOf<EventsModel.Event>(ev => ev.Payload)}
                    }
                },
                {
                    "NewEvent", new FieldResolversMap
                    {
                        {"type", Resolve.PropertyOf<EventsModel.NewEvent>(type => type.Type)},
                        {"payload", Resolve.PropertyOf<EventsModel.NewEvent>(type => type.Payload)}
                    }
                },
                {
                    "Query", new FieldResolversMap
                    {
                        {"events", context => new ValueTask<IResolveResult>(Resolve.As(Model.Events))}
                    }
                },
                {
                    "Mutation", new FieldResolversMap
                    {
                        {
                            "create", async context =>
                            {
                                var newEvent = context.GetArgument<EventsModel.NewEvent>("event");

                                if (newEvent.Payload == null)
                                    return Resolve.As(
                                        context.Schema.GetNamedType<ObjectType>("Failure"),
                                        new EventsModel.Failure("Payload should be given"));

                                var id = await Model.AddAsync(newEvent);
                                var ev = Model.Events.Single(e => e.Id == id);

                                return Resolve.As(
                                    context.Schema.GetNamedType<ObjectType>("Success"),
                                    new EventsModel.Success(id, ev));
                            }
                        }
                    }
                },
                {
                    "Subscription", new FieldResolversMap
                    {
                        {
                            "events", async (context, unsubscribe) =>
                            {
                                await Task.Delay(0);
                                var source = Model.Subscribe(unsubscribe);
                                return source;
                            },
                            context => new ValueTask<IResolveResult>(Resolve.As(context.ObjectValue))
                        }
                    }
                }
            };

            Schema = SchemaTools.MakeExecutableSchema(
                new SchemaBuilder().Sdl(Parser.ParseDocument(Sdl)),
                Resolvers,
                Resolvers);
        }

        public TypeMap Resolvers { get; set; }

        public EventsModel Model { get; set; }

        public ISchema Schema { get; set; }

        private static Dictionary<string, object> NewEvent(EventsModel.EventType type, string payload)
        {
            return new Dictionary<string, object>
            {
                {
                    "event", new Dictionary<string, object>
                    {
                        {"type", type},
                        {"payload", payload}
                    }
                }
            };
        }

        [Fact]
        public async Task Mutation1()
        {
            /* Given */
            var mutation = Parser.ParseDocument(
                @"mutation AddEvent($event: NewEvent!) {
                    create(event: $event) {
                        __typename
                        ...on Success {
                            id
                            event {
                                payload
                            }
                        }
                        
                        ...on Failure {
                            message
                        }
                    }
                }");

            var variables = NewEvent(EventsModel.EventType.INSERT, "payload");

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = Schema,
                Document = mutation,
                VariableValues = variables
            });

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""create"": {
                      ""__typename"": ""Success"",
                      ""id"": ""1"",
                      ""event"": {
                        ""payload"": ""payload""
                      }
                    }
                  }
                }");
        }

        [Fact]
        public async Task Mutation2()
        {
            /* Given */
            var mutation = Parser.ParseDocument(
                @"mutation AddEvent($event: NewEvent!) {
                    create(event: $event) {
                        __typename
                        ...on Success {
                            id
                        }
                        
                        ...on Failure {
                            message
                        }
                    }
                }");

            var variables = NewEvent(EventsModel.EventType.INSERT, null);

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = Schema,
                Document = mutation,
                VariableValues = variables
            });

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""create"": {
                      ""__typename"": ""Failure"",
                      ""message"": ""Payload should be given""
                    }
                  }
                }");
        }

        [Fact]
        public async Task Query()
        {
            /* Given */
            await Model.AddAsync(new EventsModel.NewEvent()
            {
                Type = EventsModel.EventType.DELETE,
                Payload = "payload1"
            });

            await Model.AddAsync(new EventsModel.NewEvent()
            {
                Type = EventsModel.EventType.UPDATE,
                Payload = "payload2"
            });

            var query = Parser.ParseDocument(
                @"{
                    events {
                        __typename
                        id
                        type
                        payload
                    }
                }");

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = Schema,
                Document = query
            });

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""events"": [
                      {
                        ""type"": ""DELETE"",
                        ""__typename"": ""Event"",
                        ""payload"": ""payload1"",
                        ""id"": ""1""
                      },
                      {
                        ""type"": ""UPDATE"",
                        ""__typename"": ""Event"",
                        ""payload"": ""payload2"",
                        ""id"": ""2""
                      }
                    ]
                  }
                }");
        }

        [Fact]
        public async Task Subscription()
        {
            /* Given */
            var subscription = Parser.ParseDocument(
                @"subscription {
                    events {
                        __typename
                        id
                        type
                        payload
                    }
                }");

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            /* When */
            var result = await Executor.SubscribeAsync(new ExecutionOptions()
            {
                Schema = Schema,
                Document = subscription
            }, cts.Token);

            await Model.AddAsync(new EventsModel.NewEvent()
            {
                Type = EventsModel.EventType.DELETE,
                Payload = "payload1"
            });

            var ev = await result.Source.Reader.ReadAsync(cts.Token);

            // unsubscribe
            cts.Cancel();

            /* Then */
            ev.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""events"": {
                      ""type"": ""DELETE"",
                      ""id"": ""1"",
                      ""__typename"": ""Event"",
                      ""payload"": ""payload1""
                    }
                  }
                }");
        }
    }
}
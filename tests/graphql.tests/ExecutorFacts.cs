using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Xunit;
using Xunit.Abstractions;

namespace Tanka.GraphQL.Tests;

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
        Model = new();
        Resolvers = new()
        {
            {
                "Success", new FieldResolversMap
                {
                    { "id", context => context.ResolveAsPropertyOf<EventsModel.Success>(m => m.Id) },
                    { "event", context => context.ResolveAsPropertyOf<EventsModel.Success>(m => m.Event) }
                }
            },
            {
                "Failure", new FieldResolversMap
                {
                    { "message", context => context.ResolveAsPropertyOf<EventsModel.Failure>(m => m.Message) }
                }
            },
            {
                "Event", new FieldResolversMap
                {
                    { "id", context => context.ResolveAsPropertyOf<EventsModel.Event>(ev => ev.Id) },
                    { "type", context => context.ResolveAsPropertyOf<EventsModel.Event>(ev => ev.Type) },
                    { "payload", context => context.ResolveAsPropertyOf<EventsModel.Event>(ev => ev.Payload) }
                }
            },
            {
                "NewEvent", new FieldResolversMap
                {
                    { "type", context => context.ResolveAsPropertyOf<EventsModel.NewEvent>(type => type.Type) },
                    { "payload", context => context.ResolveAsPropertyOf<EventsModel.NewEvent>(type => type.Payload) }
                }
            },
            {
                "Query", new FieldResolversMap
                {
                    { "events", context => context.ResolveAs(Model.Events) }
                }
            },
            {
                "Mutation", new FieldResolversMap
                {
                    {
                        "create", async context =>
                        {
                            var newEvent = context.BindInputObject<EventsModel.NewEvent>("event");

                            if (newEvent.Payload == null)
                            {
                                context.ResolvedValue = new EventsModel.Failure("Failure");
                                context.ResolveAbstractType = (definition, o) =>
                                    context.Schema.GetRequiredNamedType<ObjectDefinition>("Failure");
                                return;
                            }

                            var id = await Model.AddAsync(newEvent);
                            var ev = Model.Events.Single(e => e.Id == id);


                            context.ResolvedValue = new EventsModel.Success(id, ev);
                            context.ResolveAbstractType = (definition, o) =>
                                context.Schema.GetRequiredNamedType<ObjectDefinition>("Success");
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
                            context.ResolvedValue = Model.Subscribe(unsubscribe);
                        },
                        context => context.ResolveAs(context.ObjectValue)
                    }
                }
            }
        };

        Schema = new SchemaBuilder()
            .Add(Sdl)
            .Build(Resolvers, Resolvers).Result;
    }

    public EventsModel Model { get; set; }

    public ResolversMap Resolvers { get; set; }

    public ISchema Schema { get; set; }

    [Fact]
    public async Task Mutation1()
    {
        /* Given */
        var mutation =
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
                }";

        var variables = NewEvent(EventsModel.EventType.INSERT, "payload");

        /* When */
        var result = await Executor.Execute(Schema, mutation, variables);

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
        var mutation =
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
                }";

        var variables = NewEvent(EventsModel.EventType.INSERT, null);

        /* When */
        var result = await Executor.Execute(Schema, mutation, variables);

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
        await Model.AddAsync(new()
        {
            Type = EventsModel.EventType.DELETE,
            Payload = "payload1"
        });

        await Model.AddAsync(new()
        {
            Type = EventsModel.EventType.UPDATE,
            Payload = "payload2"
        });

        var query =
            @"{
                    events {
                        __typename
                        id
                        type
                        payload
                    }
                }";

        /* When */
        var result = await Executor.Execute(Schema, query);

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
        var subscription =
            @"subscription {
                    events {
                        __typename
                        id
                        type
                        payload
                    }
                }";

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        /* When */
        var result = new Executor(Schema).SubscribeAsync(new GraphQLRequest()
        {
            Document = subscription
        }, cts.Token).GetAsyncEnumerator(cts.Token);

        await Model.AddAsync(new()
        {
            Type = EventsModel.EventType.DELETE,
            Payload = "payload1"
        });

        await result.MoveNextAsync();
        var ev = result.Current;

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

    private static Dictionary<string, object> NewEvent(EventsModel.EventType type, string payload)
    {
        return new()
        {
            {
                "event", new Dictionary<string, object>
                {
                    { "type", type },
                    { "payload", payload }
                }
            }
        };
    }
}
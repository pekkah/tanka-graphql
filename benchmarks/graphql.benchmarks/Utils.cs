﻿using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Benchmarks
{
    public static class Utils
    {
        public static ISchema InitializeSchema()
        {
            var events = new SingleValueEventChannel();
            var builder = new SchemaBuilder()
                .Sdl(Parser.ParseTypeSystemDocument(
                @"
                    type Query {
                        simple: String
                    }

                    type Mutation {
                        simple: String
                    }

                    type Subscription {
                        simple: String
                    }

                    schema {
                        query: Query
                        mutation: Mutation
                        subscription: Subscription
                    }
                    "));

            var resolvers = new ObjectTypeMap
            {
                {
                    "Query", new FieldResolversMap
                    {
                        {"simple", context => new ValueTask<IResolverResult>(Resolve.As("value"))}
                    }
                },
                {
                    "Mutation", new FieldResolversMap
                    {
                        {"simple", context => new ValueTask<IResolverResult>(Resolve.As("value"))}
                    }
                },
                {
                    "Subscription", new FieldResolversMap()
                    {
                        {
                            "simple", 
                            (context, unsubscribe) => ResolveSync.Subscribe(events, unsubscribe), 
                            context => new ValueTask<IResolverResult>(Resolve.As(context.ObjectValue))}
                    }
                }
            };

            var schema = SchemaTools.MakeExecutableSchema(
                builder, 
                resolvers,
                resolvers);

            return schema;
        }

        public static ExecutableDocument InitializeQuery()
        {
            return Parser.ParseDocument(@"
{
    simple
}");
        }

        public static ExecutableDocument InitializeMutation()
        {
            return Parser.ParseDocument(@"
mutation {
    simple
}");
        }

        public static ExecutableDocument InitializeSubscription()
        {
            return Parser.ParseDocument(@"
subscription {
    simple
}");
        }
    }
}
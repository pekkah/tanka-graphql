﻿using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using tanka.graphql.resolvers;
using tanka.graphql.tools;
using tanka.graphql.type;
using GraphQLParser.AST;
using tanka.graphql.sdl;

namespace tanka.graphql.benchmarks
{
    public static class Utils
    {
        public static ISchema InitializeSchema()
        {
            var builder = new SchemaBuilder();
            Sdl.Import(Parser.ParseDocument(
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
                    "), builder);

            var resolvers = new ResolverMap
            {
                {
                    "Query", new FieldResolverMap
                    {
                        {"simple", context => new ValueTask<IResolveResult>(Resolve.As("value"))}
                    }
                },
                {
                    "Mutation", new FieldResolverMap
                    {
                        {"simple", context => new ValueTask<IResolveResult>(Resolve.As("value"))}
                    }
                },
                {
                    "Subscription", new FieldResolverMap()
                    {
                        {
                            "simple", 
                            (context, unsubscribe) => new ValueTask<ISubscribeResult>(Resolve.Stream(SimpleValueBlock("value"))), 
                            context => new ValueTask<IResolveResult>(Resolve.As(context.ObjectValue))}
                    }
                }
            };

            var schema = SchemaTools.MakeExecutableSchema(
                builder, 
                resolvers,
                resolvers);

            return schema;
        }

        private static ISourceBlock<object> SimpleValueBlock(string value)
        {
            var target = new BufferBlock<string>();
            target.Post(value);
            return target;
        }

        public static GraphQLDocument InitializeQuery()
        {
            return Parser.ParseDocument(@"
{
    simple
}");
        }

        public static GraphQLDocument InitializeMutation()
        {
            return Parser.ParseDocument(@"
mutation {
    simple
}");
        }

        public static GraphQLDocument InitializeSubscription()
        {
            return Parser.ParseDocument(@"
subscription {
    simple
}");
        }
    }
}
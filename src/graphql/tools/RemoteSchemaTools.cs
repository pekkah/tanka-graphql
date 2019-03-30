using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.channels;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.tools
{
    public delegate ValueTask<ChannelReader<ExecutionResult>> ExecutionResultLink(
        GraphQLDocument document,
        IDictionary<string, object> variables,
        CancellationToken cancellationToken);

    public static class RemoteSchemaTools
    {
        public static ISchema MakeRemoteExecutable(
            SchemaBuilder builder,
            ExecutionResultLink link,
            Func<ExecutionResultLink, Resolver> createResolver = null,
            Func<ExecutionResultLink, Subscriber> createSubscriber = null)
        {
            if (createResolver == null)
                createResolver = DefaultCreateRemoteResolver;

            if (createSubscriber == null)
                createSubscriber = DefaultCreateRemoteSubscriber;

            // add remote resolver for query
            if (builder.TryGetType<ObjectType>("Query", out var queryType))
            {
                builder.Connections(connections =>
                {
                    var fields = connections.VisitFields(queryType);

                    foreach (var field in fields)
                    {
                        var resolver = connections.GetOrAddResolver(queryType, field.Key);
                        resolver.Run(createResolver(link));
                    }
                });
            }

            if (builder.TryGetType<ObjectType>("Mutation", out var mutationType))
            {
                builder.Connections(connections =>
                {
                    var fields = connections.VisitFields(mutationType);

                    foreach (var field in fields)
                    {
                        var resolver = connections.GetOrAddResolver(mutationType, field.Key);
                        resolver.Run(createResolver(link));
                    }
                });
            }

            return builder.Build();
        }

        private static Subscriber DefaultCreateRemoteSubscriber(ExecutionResultLink link)
        {
            return async (context, unsubscribe) =>
            {
                var document = CreateDocument(context);
                var variables = context.ExecutionContext.CoercedVariableValues;

                var result = await link(document, variables, unsubscribe);
                var stream = new EventChannel<ExecutionResult>();

                var _ = Task.Run(async () =>
                {
                    while (await result.WaitToReadAsync(unsubscribe))
                    {
                        if (result.TryRead(out var executionResult))
                        {
                            await stream.WriteAsync(
                                executionResult);
                        }
                    }

                    await result.Completion;
                }, unsubscribe);

                return Resolve.Subscribe(stream, unsubscribe);
            };

            GraphQLDocument CreateDocument(ResolverContext context)
            {
                return context.ExecutionContext.Document;
            }
        }

        private static Resolver DefaultCreateRemoteResolver(ExecutionResultLink link)
        {
            return async context =>
            {
                var document = CreateDocument(context);
                var variables = context.ExecutionContext.CoercedVariableValues;

                var reader = await link(document, variables, CancellationToken.None);
                while (await reader.WaitToReadAsync(CancellationToken.None))
                {
                    if (reader.TryRead(out var executionResult))
                    {
                        return new ExecutionResultResolveResult(executionResult);
                    }
                }

                //todo
                return null;
            };

            GraphQLDocument CreateDocument(ResolverContext context)
            {
                return context.ExecutionContext.Document;
            }
        }
    }
}
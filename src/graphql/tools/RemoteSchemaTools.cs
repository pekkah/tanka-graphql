using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;
using tanka.graphql.channels;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.tools
{
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

            foreach (var objectType in builder.VisitTypes<ObjectType>())
            {
                builder.Connections(connections =>
                {
                    foreach (var field in connections.VisitFields(objectType))
                    {
                        if(!connections.TrGetResolver(objectType, field.Key, out _))
                        {
                            var resolver = connections.GetOrAddResolver(objectType, field.Key);
                            resolver.Run(context =>
                            {
                                object value = null;
                                if (context.ObjectValue is IDictionary<string, object> dictionary)
                                {
                                    value = dictionary[context.FieldName];
                                }
                                else if(context.ObjectValue is KeyValuePair<string, object> keyValue)
                                {
                                    value = keyValue.Value;
                                }

                                if (value is IDictionary<string, object>)
                                {
                                    return ResolveSync.As(value);
                                }

                                if (value is IEnumerable enumerable && !(value is string))
                                {
                                    return ResolveSync.As(enumerable);
                                }

                                return ResolveSync.As(value);
                            });
                        }
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

        public static Resolver DefaultCreateRemoteResolver(ExecutionResultLink link)
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
                        if (executionResult.Errors != null && executionResult.Errors.Any())
                        {
                            var first = executionResult.Errors.First();
                            throw new CompleteValueException(
                                $"{first.Message}",
                                nodes:new []{ context.Selection},
                                locations: new []{context.Selection.Location},
                                path: context.Path,
                                extensions: new Dictionary<string, object>()
                                {
                                    ["remoteError"] = new
                                    {
                                        data = executionResult.Data,
                                        errors = executionResult.Errors,
                                        extensions = executionResult.Extensions
                                    } 
                                });
                        }

                        return new PreExecutedResolveResult(executionResult.Data);
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server.Links;

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

        throw new NotImplementedException($"todo: {nameof(MakeRemoteExecutable)}");
    }

    public static Resolver DefaultCreateRemoteResolver(ExecutionResultLink link)
    {
        return async context =>
        {
            var document = CreateDocument(context);
            var variables = context.ExecutionContext.CoercedVariableValues;

            var reader = await link(document, variables, CancellationToken.None);
            while (await reader.WaitToReadAsync(CancellationToken.None))
                if (reader.TryRead(out var executionResult))
                    return new PreExecutedResolverResult(executionResult);

            throw new QueryExecutionException(
                "Could not get result from remote. " +
                "Link channel was closed before result could be read.",
                context.Path,
                context.Selection);
        };

        ExecutableDocument CreateDocument(IResolverContext context)
        {
            return context.ExecutionContext.Document;
        }
    }

    private static Resolver DefaultDictionaryResolver()
    {
        return context =>
        {
            object value = null;
            if (context.ObjectValue is IDictionary<string, object> dictionary)
                value = dictionary[context.FieldName];
            else if (context.ObjectValue is KeyValuePair<string, object> keyValue)
                value = keyValue.Value;
            else if (context.ObjectValue is ExecutionResult er)
                return new ValueTask<IResolverResult>(new PreExecutedResolverResult(er));

            if (value is IDictionary<string, object>) return ResolveSync.As(value);

            if (value is IEnumerable enumerable && !(value is string)) return ResolveSync.As(enumerable);

            return ResolveSync.As(value);
        };
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
                    if (result.TryRead(out var executionResult))
                        await stream.WriteAsync(
                            executionResult);

                await result.Completion;
            }, unsubscribe);

            return Resolve.Subscribe(stream, unsubscribe);
        };

        ExecutableDocument CreateDocument(IResolverContext context)
        {
            return context.ExecutionContext.Document;
        }
    }
}
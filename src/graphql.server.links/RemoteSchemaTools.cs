using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server.Links;

public static class RemoteSchemaTools
{
    private static IReadOnlyList<string> RootTypes = new List<string>()
    {
        "Query",
        "Mutation",
        "Subscription"
    };

    //todo: use ResolversBuilder.AddLinkedTypes()
    /// <summary>
    ///     Create resolvers for ObjectTypes and ObjectType extensions in <see cref="TypeSystemDocument"/> which
    ///     resolve the values using the given <paramref name="link"/>
    /// </summary>
    /// <param name="remoteTypes"></param>
    /// <param name="link"></param>
    /// <param name="createResolver"></param>
    /// <param name="createSubscriber"></param>
    /// <returns></returns>
    public static ResolversMap CreateLinkResolvers(
        TypeSystemDocument remoteTypes,
        ExecutionResultLink link,
        Func<ExecutionResultLink, Resolver> createResolver = null,
        Func<ExecutionResultLink, Subscriber> createSubscriber = null)
    {
        if (createResolver == null)
            createResolver = DefaultCreateRemoteResolver;

        if (createSubscriber == null)
            createSubscriber = DefaultCreateRemoteSubscriber;

        var objectDefinitionsAndExtensions = GetObjectDefinitions(remoteTypes);
        var rootTypes = objectDefinitionsAndExtensions.Where(type => RootTypes.Contains(type.Name.Value))
            .OfType<ObjectDefinition>();

        ResolversMap resolvers = new ResolversMap();
        foreach(var rootType in rootTypes)
        {
            foreach(var field in rootType.Fields)
            {
                if (rootType.Name != "Subscription")
                {
                    resolvers.Add(rootType.Name.Value, field.Name.Value, createResolver(link));
                }
                else
                {
                    resolvers.Add(rootType.Name.Value, field.Name.Value, createSubscriber(link));
                }
            }            
        }

        var resolver = DefaultDictionaryResolver();
        foreach (var objectType in objectDefinitionsAndExtensions.Where(type => !RootTypes.Contains(type.Name.Value))
            .OfType<ObjectDefinition>())
        {
            foreach (var field in objectType.Fields)
            {
                resolvers.Add(objectType.Name.Value, field.Name.Value, resolver);
            }
        }

        return resolvers;
    }

    private static IEnumerable<ObjectDefinition> GetObjectDefinitions(TypeSystemDocument typeSystem)
    {
        if (typeSystem.TypeDefinitions != null)
            foreach(var typeDefinition in typeSystem.TypeDefinitions)
            {
                if (typeDefinition is ObjectDefinition objectDefinition)
                    yield return objectDefinition;
            }   
        
        if (typeSystem.TypeExtensions != null)
            foreach(var typeExtension in typeSystem.TypeExtensions)
            {
                if (typeExtension.ExtendedKind == NodeKind.ObjectDefinition)
                    yield return (ObjectDefinition)typeExtension.Definition;
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
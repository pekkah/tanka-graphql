using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Directives;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem.SchemaValidation;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Middleware that handles final schema construction (terminal middleware)
/// </summary>
public class FinalizationMiddleware : ISchemaBuildMiddleware
{
    private static readonly Dictionary<string, FieldDefinition> NoFields = new(0);
    private static readonly Dictionary<string, InputValueDefinition> NoInputObjectFields = new(0);
    private static readonly List<Directive> NoDirectives = new(0);

    public Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        var options = context.Options;
        var resolvers = context.Resolvers.BuildResolvers();
        var subscribers = context.Resolvers.BuildSubscribers();

        var typeDefinitions = context.Builder.GetTypeDefinitions(context.Options.BuildTypesFromOrphanedExtensions);
        var namedTypeDefinitions = typeDefinitions
            .ToDictionary(type => type.Name.Value, type => type);

        var schemas = context.Builder.GetSchemaDefinitions();
        var schemasList = schemas.ToList();
        var operationDefinitions = schemasList
            .SelectMany(schema => schema.Operations)
            .ToList();

        var queryRoot = FindQueryRoot(
            namedTypeDefinitions,
            operationDefinitions,
            options.OverrideQueryRootName,
            context.Builder
        );

        var mutationRoot = FindMutationRoot(
            namedTypeDefinitions,
            operationDefinitions,
            options.OverrideMutationRootName,
            context.Builder
        );

        var subscriptionRoot = FindSubscriptionRoot(
            namedTypeDefinitions,
            operationDefinitions,
            options.OverrideSubscriptionRootName,
            context.Builder
        );

        var allFields = namedTypeDefinitions
            .Where(kv => kv.Value is ObjectDefinition)
            .ToDictionary(
                kv => kv.Key,
                kv => ((ObjectDefinition)kv.Value).Fields?.ToDictionary(field => field.Name.Value, field => field) ??
                      NoFields
            );

        var interfaceFields = namedTypeDefinitions
            .Where(kv => kv.Value is InterfaceDefinition)
            .ToDictionary(
                kv => kv.Key,
                kv => ((InterfaceDefinition)kv.Value).Fields?.ToDictionary(field => field.Name.Value, field => field) ??
                      NoFields
            );

        foreach (var (type, fields) in interfaceFields) allFields.Add(type, fields);

        var inputFields = namedTypeDefinitions
            .Where(kv => kv.Value is InputObjectDefinition)
            .ToDictionary(
                kv => kv.Key,
                kv =>
                    ((InputObjectDefinition)kv.Value).Fields?.ToDictionary(field => field.Name.Value, field => field) ??
                    NoInputObjectFields
            );

        var schemaDirectives = schemasList
            ?.SelectMany(schema => schema.Directives?.ToList() ?? NoDirectives)
            .ToList();

        var directiveDefinitions = context.Builder.GetDirectiveDefinitions()
            .ToDictionary(dd => dd.Name.Value, dd => dd);

        ISchema schema = new ExecutableSchema(
            namedTypeDefinitions,
            allFields,
            inputFields,
            directiveDefinitions,
            queryRoot,
            resolvers,
            options.ValueConverters ?? new Dictionary<string, IValueConverter>(0),
            mutationRoot,
            subscriptionRoot,
            subscribers,
            schemaDirectives
        );

        return Task.FromResult(schema);
    }

    private ObjectDefinition FindQueryRoot(
        IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
        IEnumerable<RootOperationTypeDefinition> operationDefinitions,
        string? overrideQueryRootName,
        SchemaBuilder builder)
    {
        if (!string.IsNullOrEmpty(overrideQueryRootName))
            return (ObjectDefinition)typeDefinitions[overrideQueryRootName];

        var queryNamedType = operationDefinitions
            .SingleOrDefault(op => op.OperationType == OperationType.Query)
            ?.NamedType;

        // by convention
        if (queryNamedType == null && typeDefinitions.TryGetValue("Query", out var queryDefinition))
            return (ObjectDefinition)queryDefinition;

        if (queryNamedType is null)
            throw new InvalidOperationException("Could not find query operation");

        return (ObjectDefinition)typeDefinitions[queryNamedType.Name];
    }

    private ObjectDefinition? FindMutationRoot(
        IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
        IEnumerable<RootOperationTypeDefinition> operationDefinitions,
        string? overrideMutationRootName,
        SchemaBuilder builder)
    {
        if (!string.IsNullOrEmpty(overrideMutationRootName))
            return (ObjectDefinition)typeDefinitions[overrideMutationRootName];

        var mutationNamedType = operationDefinitions
            .SingleOrDefault(op => op.OperationType == OperationType.Mutation)
            ?.NamedType;

        // by convention
        if (mutationNamedType == null && typeDefinitions.TryGetValue("Mutation", out var mutationDefinition))
            return (ObjectDefinition)mutationDefinition;

        if (mutationNamedType is null)
            return null;

        return (ObjectDefinition)typeDefinitions[mutationNamedType.Name];
    }

    private ObjectDefinition? FindSubscriptionRoot(
        IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
        IEnumerable<RootOperationTypeDefinition> operationDefinitions,
        string? overrideSubscriptionRootName,
        SchemaBuilder builder)
    {
        if (!string.IsNullOrEmpty(overrideSubscriptionRootName))
            return (ObjectDefinition)typeDefinitions[overrideSubscriptionRootName];

        var subscriptionNamedType = operationDefinitions
            .SingleOrDefault(op => op.OperationType == OperationType.Subscription)
            ?.NamedType;

        // by convention
        if (subscriptionNamedType == null &&
            typeDefinitions.TryGetValue("Subscription", out var subscriptionDefinition))
            return (ObjectDefinition)subscriptionDefinition;

        if (subscriptionNamedType is null)
            return null;

        return (ObjectDefinition)typeDefinitions[subscriptionNamedType.Name];
    }
}
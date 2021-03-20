using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public class ExecutableSchemaBuilder
    {
        private readonly ConcurrentDictionary<string, DirectiveDefinition> _directiveDefinitions = new();

        private readonly ConcurrentBag<SchemaDefinition> _schemaDefinitions = new();
        private readonly ConcurrentBag<SchemaExtension> _schemaExtensions = new();

        private readonly ConcurrentDictionary<string, TypeDefinition> _typeDefinitions = new();
        private readonly ConcurrentDictionary<string, ConcurrentBag<TypeExtension>> _typeExtensions = new();


        public ExecutableSchemaBuilder Add(TypeSystemDocument typeSystem)
        {
            if (typeSystem.SchemaDefinitions != null)
                foreach (var schemaDefinition in typeSystem.SchemaDefinitions)
                    Add(schemaDefinition);

            if (typeSystem.SchemaExtensions != null)
                foreach (var schemaExtension in typeSystem.SchemaExtensions)
                    Add(schemaExtension);

            if (typeSystem.TypeDefinitions != null)
                foreach (var typeDefinition in typeSystem.TypeDefinitions)
                    TryAdd(typeDefinition);

            if (typeSystem.TypeExtensions != null)
                foreach (var typeExtension in typeSystem.TypeExtensions)
                    Add(typeExtension);

            return this;
        }

        public ExecutableSchemaBuilder Add(SchemaDefinition schemaDefinition)
        {
            _schemaDefinitions.Add(schemaDefinition);
            return this;
        }

        public ExecutableSchemaBuilder Add(SchemaExtension schemaExtension)
        {
            _schemaExtensions.Add(schemaExtension);
            return this;
        }

        public bool TryAdd(TypeDefinition typeDefinition)
        {
            return _typeDefinitions.TryAdd(typeDefinition.Name, typeDefinition);
        }

        public bool TryAdd(TypeDefinition[] typeDefinitions)
        {
            foreach (var typeDefinition in typeDefinitions)
                if (!TryAdd(typeDefinition))
                    return false;

            return true;
        }

        public bool TryAdd(DirectiveDefinition directiveDefinition)
        {
            return _directiveDefinitions.TryAdd(directiveDefinition.Name, directiveDefinition);
        }

        public bool TryAdd(DirectiveDefinition[] directiveDefinitions)
        {
            foreach (var directiveDefinition in directiveDefinitions)
                if (!TryAdd(directiveDefinition))
                    return false;

            return true;
        }

        public ExecutableSchemaBuilder Add(TypeExtension typeExtension)
        {
            var typeExtensions = _typeExtensions
                .GetOrAdd(typeExtension.Name, _ => new ConcurrentBag<TypeExtension>());

            typeExtensions.Add(typeExtension);

            return this;
        }

        public ExecutableSchemaBuilder Add(TypeExtension[] typeExtensions)
        {
            foreach (var typeExtension in typeExtensions) Add(typeExtension);

            return this;
        }

        public ExecutableSchema Build(SchemaBuildOptions? options = null)
        {
            options ??= new SchemaBuildOptions();

            var typeDefinitions = BuildTypeDefinitions(
                options.BuildTypesFromOrphanedExtensions
            ).ToDictionary(type => type.Name.Value, type => type);

            var schemas = BuildSchemas();
            var operationDefinitions = schemas
                .SelectMany(schema => schema.Operations)
                .ToList();

            var queryRoot = FindQueryRoot(
                typeDefinitions,
                operationDefinitions,
                options.OverrideQueryRootName
            );

            var mutationRoot = FindMutationRoot(
                typeDefinitions,
                operationDefinitions,
                options.OverrideMutationRootName
            );

            var subscriptionRoot = FindSubscriptionRoot(
                typeDefinitions,
                operationDefinitions,
                options.OverrideSubscriptionRootName
            );

            return new ExecutableSchema(
                queryRoot,
                mutationRoot,
                subscriptionRoot,
                typeDefinitions);
        }

        private IEnumerable<SchemaDefinition> BuildSchemas()
        {
            var extensions = _schemaExtensions.ToList();
            var extensionDirectives = extensions
                .Where(e => e.Directives is not null)
                .SelectMany(e => e.Directives)
                .ToList();

            var extensionOperations = extensions
                .Where(e => e.Operations is not null)
                .SelectMany(e => e.Operations)
                .ToList();

            foreach (var schemaDefinition in _schemaDefinitions)
            {
                var operations = schemaDefinition.Operations.ToList();
                operations.AddRange(extensionOperations);

                yield return schemaDefinition
                    .WithDirectives(schemaDefinition.Directives
                        .Concat(extensionDirectives))
                    .WithOperations(operations);
            }
        }

        private ObjectDefinition FindQueryRoot(
            IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
            IEnumerable<RootOperationTypeDefinition> operationDefinitions,
            string? overrideQueryRootName = null)
        {
            if (!string.IsNullOrEmpty(overrideQueryRootName))
                return (ObjectDefinition) typeDefinitions[overrideQueryRootName];

            var queryNamedType = operationDefinitions
                .SingleOrDefault(op => op.OperationType == OperationType.Query)
                ?.NamedType;

            if (queryNamedType is null)
                throw new InvalidOperationException("Could not find query operation");

            return (ObjectDefinition) typeDefinitions[queryNamedType.Name];
        }

        private ObjectDefinition? FindMutationRoot(
            IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
            IEnumerable<RootOperationTypeDefinition> operationDefinitions,
            string? overrideMutationRootName = null)
        {
            if (!string.IsNullOrEmpty(overrideMutationRootName))
                return (ObjectDefinition) typeDefinitions[overrideMutationRootName];

            var mutationNamedType = operationDefinitions
                .SingleOrDefault(op => op.OperationType == OperationType.Mutation)
                ?.NamedType;

            if (mutationNamedType is null)
                return null;

            return (ObjectDefinition) typeDefinitions[mutationNamedType.Name];
        }

        private ObjectDefinition? FindSubscriptionRoot(
            IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
            IEnumerable<RootOperationTypeDefinition> operationDefinitions,
            string? overrideSubscriptionRootName = null)
        {
            if (!string.IsNullOrEmpty(overrideSubscriptionRootName))
                return (ObjectDefinition) typeDefinitions[overrideSubscriptionRootName];

            var subscriptionNamedType = operationDefinitions
                .SingleOrDefault(op => op.OperationType == OperationType.Subscription)
                ?.NamedType;

            if (subscriptionNamedType is null)
                return null;

            return (ObjectDefinition) typeDefinitions[subscriptionNamedType.Name];
        }

        private IEnumerable<TypeDefinition> BuildTypeDefinitions(
            bool buildTypeFromOrphanedExtension)
        {
            var typesToBuild = _typeDefinitions.Values.ToList();
            var typeExtensionsToBuild = _typeExtensions
                .ToDictionary(
                    types => types.Key,
                    types => types.Value.ToList());


            foreach (var type in typesToBuild)
                if (typeExtensionsToBuild.TryGetValue(type.Name, out var extensions))
                {
                    yield return type.Extend(extensions.ToArray());
                    typeExtensionsToBuild.Remove(type.Name);
                }
                else
                {
                    yield return type;
                }

            // build types from orphaned extensions
            if (buildTypeFromOrphanedExtension && typeExtensionsToBuild.Count > 0)
                foreach (var typeExtension in typeExtensionsToBuild.ToList())
                {
                    var (_, extensions) = typeExtension;

                    if (extensions.Count == 0)
                        continue;

                    // pick first extension as the type
                    var type = extensions[0].Definition;
                    extensions.RemoveAt(0);

                    yield return type.Extend(extensions.ToArray());
                }
        }
    }

    public class SchemaBuildOptions
    {
        public bool BuildTypesFromOrphanedExtensions { get; set; } = false;
        public string? OverrideQueryRootName { get; set; }
        public string? OverrideMutationRootName { get; set; }
        public string? OverrideSubscriptionRootName { get; set; }
    }
}
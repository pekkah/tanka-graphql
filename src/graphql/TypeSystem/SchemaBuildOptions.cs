using System;
using System.Collections.Generic;
using Tanka.GraphQL.Directives;
using Tanka.GraphQL.Extensions;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.ImportProviders;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

public class SchemaBuildOptions
{
    public bool BuildTypesFromOrphanedExtensions { get; set; } = false;

    public IReadOnlyDictionary<string, CreateDirectiveVisitor>? DirectiveVisitorFactories { get; set; }

    public string? OverrideMutationRootName { get; set; }

    public string? OverrideQueryRootName { get; set; }

    public string? OverrideSubscriptionRootName { get; set; }

    public IResolverMap? Resolvers { get; set; }

    public ISubscriberMap? Subscribers { get; set; }

    public IReadOnlyDictionary<string, IValueConverter>? ValueConverters { get; set; } =
        new Dictionary<string, IValueConverter>
        {
            [Scalars.String.Name] = new StringConverter(),
            [Scalars.Int.Name] = new IntConverter(),
            [Scalars.Float.Name] = new DoubleConverter(),
            [Scalars.Boolean.Name] = new BooleanConverter(),
            [Scalars.ID.Name] = new IdConverter()
        };

    public IReadOnlyList<IImportProvider> ImportProviders { get; set; } = new List<IImportProvider>()
    {
        new EmbeddedResourceImportProvider(),
        new FileSystemImportProvider(AppContext.BaseDirectory),
        new ExtensionsImportProvider()
    };

    public bool IncludeIntrospection { get; set; } = true;
}
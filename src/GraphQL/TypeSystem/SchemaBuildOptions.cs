using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Directives;
using Tanka.GraphQL.TypeSystem.SchemaValidation;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Options for schema build pipeline that also serves as the pipeline builder
/// </summary>
public class SchemaBuildOptions
{
    private readonly Dictionary<SchemaBuildStage, List<PipelineComponent>> _stageComponents = new();

    /// <summary>
    /// Schema loader for @link directive processing
    /// </summary>
    public ISchemaLoader? SchemaLoader { get; set; }

    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    public IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Build types from orphaned extensions
    /// </summary>
    public bool BuildTypesFromOrphanedExtensions { get; set; } = false;

    /// <summary>
    /// Directive visitor factories for schema transformations
    /// </summary>
    public IReadOnlyDictionary<string, CreateDirectiveVisitor>? DirectiveVisitorFactories { get; set; }

    /// <summary>
    /// Include built-in types (String, Int, etc.)
    /// </summary>
    public bool IncludeBuiltInTypes { get; set; } = true;

    /// <summary>
    /// Include introspection types and resolvers
    /// </summary>
    public bool IncludeIntrospection { get; set; } = true;

    /// <summary>
    /// Override the query root type name
    /// </summary>
    public string? OverrideQueryRootName { get; set; }

    /// <summary>
    /// Override the mutation root type name
    /// </summary>
    public string? OverrideMutationRootName { get; set; }

    /// <summary>
    /// Override the subscription root type name
    /// </summary>
    public string? OverrideSubscriptionRootName { get; set; }

    /// <summary>
    /// Resolvers for the schema
    /// </summary>
    public IResolverMap? Resolvers { get; set; }

    /// <summary>
    /// Subscribers for subscriptions
    /// </summary>
    public ISubscriberMap? Subscribers { get; set; }

    /// <summary>
    /// Schema validation rules
    /// </summary>
    public IEnumerable<SchemaValidationRule>? SchemaValidationRules { get; set; } = BuiltInSchemaValidationRules.BuildAll();

    /// <summary>
    /// Value converters for scalar types
    /// </summary>
    public Dictionary<string, IValueConverter>? ValueConverters { get; set; } =
        new Dictionary<string, IValueConverter>
        {
            [Scalars.String.Name] = new StringConverter(),
            [Scalars.Int.Name] = new IntConverter(),
            [Scalars.Float.Name] = new DoubleConverter(),
            [Scalars.Boolean.Name] = new BooleanConverter(),
            [Scalars.ID.Name] = new IdConverter()
        };

    /// <summary>
    /// Maximum depth for @link directive processing
    /// </summary>
    public int MaxLinkDepth { get; set; } = 10;

    /// <summary>
    /// Default schema build options with standard middlewares
    /// </summary>
    public static SchemaBuildOptions Default => new SchemaBuildOptions()
        .Use<BuiltInTypesMiddleware>(SchemaBuildStage.TypeCollection)
        .Use<LinkProcessingMiddleware>(SchemaBuildStage.LinkProcessing)
        .Use<ApplyDirectivesMiddleware>(SchemaBuildStage.TypeResolution)
        .Use<IntrospectionMiddleware>(SchemaBuildStage.TypeResolution)
        .Use<ValidationMiddleware>(SchemaBuildStage.Validation)
        .Use<FinalizationMiddleware>(SchemaBuildStage.Finalization);

    public SchemaBuildOptions()
    {
        // Initialize all stages with empty lists
        foreach (SchemaBuildStage stage in Enum.GetValues<SchemaBuildStage>())
        {
            _stageComponents[stage] = new List<PipelineComponent>();
        }

        // Configure the default pipeline by default
        this.Use<BuiltInTypesMiddleware>(SchemaBuildStage.TypeCollection)
            .Use<LinkProcessingMiddleware>(SchemaBuildStage.LinkProcessing)
            .Use<ApplyDirectivesMiddleware>(SchemaBuildStage.TypeResolution)
            .Use<IntrospectionMiddleware>(SchemaBuildStage.TypeResolution)
            .Use<ValidationMiddleware>(SchemaBuildStage.Validation)
            .Use<FinalizationMiddleware>(SchemaBuildStage.Finalization);
    }

    /// <summary>
    /// Constructor for backwards compatibility
    /// </summary>
    public SchemaBuildOptions(IResolverMap resolvers, ISubscriberMap? subscribers = null) : this()
    {
        Resolvers = resolvers;
        Subscribers = subscribers;
    }

    /// <summary>
    /// Add middleware to the end of the specified stage
    /// </summary>
    public SchemaBuildOptions Use(
        SchemaBuildStage stage,
        Func<ISchemaBuildContext, SchemaBuildDelegate, Task<ISchema>> middleware)
    {
        _stageComponents[stage].Add(new PipelineComponent(middleware, false));
        return this;
    }

    /// <summary>
    /// Add middleware type to the end of the specified stage (resolved from DI)
    /// </summary>
    public SchemaBuildOptions Use<TMiddleware>(SchemaBuildStage stage)
        where TMiddleware : ISchemaBuildMiddleware
    {
        return Use(stage, async (context, next) =>
        {
            var middleware = context.ServiceProvider.GetService<TMiddleware>() ??
                           ActivatorUtilities.CreateInstance<TMiddleware>(context.ServiceProvider);
            return await middleware.InvokeAsync(context, next);
        });
    }

    /// <summary>
    /// Add middleware to the beginning of the specified stage
    /// </summary>
    public SchemaBuildOptions UseFirst(
        SchemaBuildStage stage,
        Func<ISchemaBuildContext, SchemaBuildDelegate, Task<ISchema>> middleware)
    {
        _stageComponents[stage].Add(new PipelineComponent(middleware, true));
        return this;
    }

    /// <summary>
    /// Add middleware type to the beginning of the specified stage
    /// </summary>
    public SchemaBuildOptions UseFirst<TMiddleware>(SchemaBuildStage stage)
        where TMiddleware : ISchemaBuildMiddleware
    {
        return UseFirst(stage, async (context, next) =>
        {
            var middleware = context.ServiceProvider.GetService<TMiddleware>() ??
                           ActivatorUtilities.CreateInstance<TMiddleware>(context.ServiceProvider);
            return await middleware.InvokeAsync(context, next);
        });
    }

    /// <summary>
    /// Build the complete pipeline from all stages
    /// </summary>
    public SchemaBuildDelegate BuildPipeline()
    {
        SchemaBuildDelegate pipeline = _ =>
        {
            throw new InvalidOperationException("Schema build pipeline completed without producing a schema");
        };

        // Process stages in reverse order (like ASP.NET Core middleware)
        var stages = Enum.GetValues<SchemaBuildStage>().Reverse();

        foreach (var stage in stages)
        {
            var components = _stageComponents[stage];

            // Within each stage, process in reverse order, but handle UseFirst correctly
            var orderedComponents = components
                .OrderBy(c => c.InsertFirst ? 0 : 1) // UseFirst components go first
                .Reverse(); // Then reverse for middleware building

            foreach (var component in orderedComponents)
            {
                var currentMiddleware = component.Middleware;
                var currentPipeline = pipeline;
                pipeline = context => currentMiddleware(context, currentPipeline);
            }
        }

        return pipeline;
    }


    private record PipelineComponent(
        Func<ISchemaBuildContext, SchemaBuildDelegate, Task<ISchema>> Middleware,
        bool InsertFirst);
}
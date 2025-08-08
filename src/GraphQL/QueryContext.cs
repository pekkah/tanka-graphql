using Microsoft.AspNetCore.Http.Features;

using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.Response;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

/// <summary>
///     GraphQL query context used by <see cref="OperationDelegate"/> for execution of the operation.
/// </summary>
public record QueryContext
{
    private FeatureReferences<FeatureInterfaces> _features;

    public QueryContext(IFeatureCollection defaults)
    {
        _features.Initalize(defaults, defaults.Revision);
    }

    public QueryContext() : this(new FeatureCollection(12))
    {
    }

    /// <summary>
    ///     Request cancellation token.
    /// </summary>
    public CancellationToken RequestCancelled { get; set; } = default;

    /// <summary>
    ///     Context features
    /// </summary>
    public IFeatureCollection Features => _features.Collection;

    private IResponseStreamFeature ResponseFeature =>
        _features.Fetch(
            ref _features.Cache.Response,
            _ => new GraphQLResponseFeature())!;

    private IGraphQLRequestFeature RequestFeature => _features.Fetch(
        ref _features.Cache.Request,
        _ => new GraphQLRequestFeature())!;

    private ICoercedVariableValuesFeature CoercedVariableValuesFeature => _features.Fetch(
        ref _features.Cache.CoercedVariableValues,
        _ => new CoercedVariableValuesFeature())!;

    private IOperationFeature OperationFeature => _features.Fetch(
        ref _features.Cache.Operation,
        _ => new OperationFeature())!;

    private ISchemaFeature SchemaFeature => _features.Fetch(
        ref _features.Cache.Schema,
        _ => new SchemaFeature())!;

    private ISelectionSetExecutorFeature? SelectionSetExecutorFeature =>
        _features.Fetch(ref _features.Cache.SelectionSetExecutor, _ => null);

    private IErrorCollectorFeature? ErrorCollectorFeature =>
        _features.Fetch(ref _features.Cache.ErrorCollector, _ => null);

    private IValueCompletionFeature? ValueCompletionFeature =>
        _features.Fetch(ref _features.Cache.ValueCompletion, _ => null);

    public IArgumentBinderFeature? ArgumentBinder =>
        _features.Fetch(ref _features.Cache.ArgumentBinder, _ => new ArgumentBinderFeature());

    private IFieldExecutorFeature? FieldExecutorFeature =>
        _features.Fetch(ref _features.Cache.FieldExecutor, _ => null);

    private IRequestServicesFeature RequestServicesFeature =>
        _features.Fetch(ref _features.Cache.RequestServices, _ => new RequestServicesFeature())!;

    /// <summary>
    /// GraphQL request
    /// </summary>
    public GraphQLRequest Request
    {
        get => RequestFeature.Request ?? throw new InvalidOperationException("Request not set");
        set => RequestFeature.Request = value;
    }

    /// <summary>
    ///     Coerced variable values
    /// </summary>
    public IReadOnlyDictionary<string, object?> CoercedVariableValues
    {
        get => CoercedVariableValuesFeature.CoercedVariableValues;
        set => CoercedVariableValuesFeature.CoercedVariableValues = value;
    }

    /// <summary>
    ///     Operation to execute
    /// </summary>
    public OperationDefinition OperationDefinition
    {
        get => OperationFeature.Operation;
        set => OperationFeature.Operation = value;
    }

    /// <summary>
    ///     Schema to execute against
    /// </summary>
    public ISchema Schema
    {
        get => SchemaFeature.Schema;
        set => SchemaFeature.Schema = value;
    }

    /// <summary>
    ///     Response stream
    /// </summary>
    public IAsyncEnumerable<ExecutionResult> Response
    {
        get => ResponseFeature.Response;
        set => ResponseFeature.Response = value;
    }

    /// <summary>
    ///     Request services
    /// </summary>
    public IServiceProvider RequestServices
    {
        get => RequestServicesFeature.RequestServices;
        set => RequestServicesFeature.RequestServices = value;
    }

    public IErrorCollectorFeature? Errors => ErrorCollectorFeature;

    /// <summary>
    ///     Add error to the context
    /// </summary>
    /// <param name="x"></param>
    public void AddError(Exception x)
    {
        ArgumentNullException.ThrowIfNull(ErrorCollectorFeature);
        ErrorCollectorFeature.Add(x);
    }

    /// <summary>
    ///     Complete resolved value
    /// </summary>
    /// <param name="context"></param>
    /// <param name="fieldType"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path)
    {
        ArgumentNullException.ThrowIfNull(ValueCompletionFeature);
        return ValueCompletionFeature.CompleteValueAsync(context, fieldType, path);
    }

    /// <summary>
    ///     Complete field value with streaming support
    /// </summary>
    /// <param name="context"></param>
    /// <param name="fieldType"></param>
    /// <param name="path"></param>
    /// <param name="initialCount">Initial count for @stream directive</param>
    /// <param name="label">Label for @stream directive</param>
    /// <returns></returns>
    public ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path,
        int initialCount,
        string? label)
    {
        ArgumentNullException.ThrowIfNull(ValueCompletionFeature);
        return ValueCompletionFeature.CompleteValueAsync(context, fieldType, path, initialCount, label);
    }

    /// <summary>
    ///     Execute field
    /// </summary>
    /// <param name="objectDefinition"></param>
    /// <param name="objectValue"></param>
    /// <param name="fields"></param>
    /// <param name="path"></param>
    /// <param name="fieldMetadata">Optional metadata for the field (e.g., defer/stream directives)</param>
    /// <returns></returns>
    public Task<object?> ExecuteField(ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path,
        IReadOnlyDictionary<string, object>? fieldMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(FieldExecutorFeature);
        return FieldExecutorFeature.Execute(this, objectDefinition, objectValue, fields, path, fieldMetadata);
    }

    /// <summary>
    ///     Execute selection set
    /// </summary>
    /// <param name="selectionSet"></param>
    /// <param name="objectType"></param>
    /// <param name="objectValue"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        SelectionSet selectionSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        ArgumentNullException.ThrowIfNull(SelectionSetExecutorFeature);
        return SelectionSetExecutorFeature.ExecuteSelectionSet(
            this,
            selectionSet,
            objectType,
            objectValue,
            path);
    }

    /// <summary>
    ///     Get errors from the context
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ExecutionError> GetErrors()
    {
        ArgumentNullException.ThrowIfNull(ErrorCollectorFeature);
        return ErrorCollectorFeature.GetErrors();
    }

    private struct FeatureInterfaces
    {
        public IGraphQLRequestFeature? Request;
        public ICoercedVariableValuesFeature? CoercedVariableValues;
        public IOperationFeature? Operation;
        public ISchemaFeature? Schema;
        public IFieldExecutorFeature? FieldExecutor;
        public ISelectionSetExecutorFeature? SelectionSetExecutor;
        public IErrorCollectorFeature? ErrorCollector;
        public IValueCompletionFeature? ValueCompletion;
        public IArgumentBinderFeature? ArgumentBinder;
        public IResponseStreamFeature? Response;
        public IRequestServicesFeature? RequestServices;
    }
}
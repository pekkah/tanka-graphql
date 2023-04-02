using Microsoft.AspNetCore.Http.Features;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.Response;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

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


    public CancellationToken RequestCancelled { get; set; } = default;

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

    public GraphQLRequest Request
    {
        get => RequestFeature.Request ?? throw new InvalidOperationException("Request not set");
        set => RequestFeature.Request = value;
    }


    public IReadOnlyDictionary<string, object?> CoercedVariableValues
    {
        get => CoercedVariableValuesFeature.CoercedVariableValues;
        set => CoercedVariableValuesFeature.CoercedVariableValues = value;
    }

    public OperationDefinition OperationDefinition
    {
        get => OperationFeature.Operation;
        set => OperationFeature.Operation = value;
    }

    public ISchema Schema
    {
        get => SchemaFeature.Schema;
        set => SchemaFeature.Schema = value;
    }

    public IAsyncEnumerable<ExecutionResult> Response
    {
        get => ResponseFeature.Response;
        set => ResponseFeature.Response = value;
    }

    //todo: turn into a feature
    public IServiceProvider RequestServices { get; set; }

    public void AddError(Exception x)
    {
        ArgumentNullException.ThrowIfNull(ErrorCollectorFeature);
        ErrorCollectorFeature.Add(x);
    }

    public ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path)
    {
        ArgumentNullException.ThrowIfNull(ValueCompletionFeature);
        return ValueCompletionFeature.CompleteValueAsync(context, fieldType, path);
    }

    public Task<object?> ExecuteField(ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path)
    {
        ArgumentNullException.ThrowIfNull(FieldExecutorFeature);
        return FieldExecutorFeature.Execute(this, objectDefinition, objectValue, fields, path);
    }

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
    }
}
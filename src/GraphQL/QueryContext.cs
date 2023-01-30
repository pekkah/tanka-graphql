using Microsoft.AspNetCore.Http.Features;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public record QueryContext
{
    private FeatureReferences<FeatureInterfaces> _features;

    public QueryContext(IFeatureCollection defaults)
    {
        _features.Initalize(defaults, defaults.Revision);
    }

    public QueryContext() : this(new FeatureCollection(4))
    {
    }

    public IFeatureCollection Features => _features.Collection;

    private IGraphQLRequestFeature RequestFeature => _features.Fetch(ref _features.Cache.Request, _ =>
        new GraphQLRequestFeature
        {
            Request = new()
            {
                Document = "{}"
            }
        })!;

    private ICoercedVariableValuesFeature CoercedVariableValuesFeature => _features.Fetch(
        ref _features.Cache.CoercedVariableValues, _ => new CoercedVariableValuesFeature
        {
            CoercedVariableValues = new Dictionary<string, object?>(0)
        })!;

    private IOperationFeature OperationFeature => _features.Fetch(ref _features.Cache.Operation, _ =>
        new OperationFeature
        {
            Operation = "{}"
        })!;

    private ISchemaFeature SchemaFeature => _features.Fetch(ref _features.Cache.Schema, _ => new SchemaFeature
    {
        Schema = ISchema.Empty
    })!;

    private IFieldExecutorFeature FieldExecutorFeature =>
        _features.Fetch(ref _features.Cache.FieldExecutor, _ => new FieldExecutorFeature
        {
            FieldExecutor = IFieldExecutor.Default
        })!;

    internal ISelectionSetExecutorFeature SelectionSetExecutorFeature => _features.Fetch(ref _features.Cache.SelectionSetExecutor, _ => null)!;

    private IErrorCollectorFeature ErrorCollectorFeature =>
        _features.Fetch(ref _features.Cache.ErrorCollector, _ => new ErrorCollectorFeature
        {
            ErrorCollector = IErrorCollector.Default()
        })!;

    internal IValueCompletionFeature ValueCompletionFeature =>
        _features.Fetch(ref _features.Cache.ValueCompletion, _ => new ValueCompletionFeature())!;

    internal IArgumentBinderFeature ArgumentBinderFeature =>
        _features.Fetch(ref _features.Cache.ArgumentBinder, _ => IArgumentBinderFeature.Default)!;

    internal IValidatorFeature? ValidatorFeature =>
        _features.Fetch(ref _features.Cache.Validator, _ => null);

    public GraphQLRequest Request
    {
        get => RequestFeature.Request;
        set => RequestFeature.Request = value;
    }

    public IFieldExecutor FieldExecutor
    {
        get => FieldExecutorFeature.FieldExecutor;
        set => FieldExecutorFeature.FieldExecutor = value;
    }

    /*
    public ISelectionSetExecutor SelectionSetExecutor
    {
        get => SelectionSetExecutorFeature.SelectionSetExecutor;
        set => SelectionSetExecutorFeature.SelectionSetExecutor = value;
    }
    */

    public IReadOnlyDictionary<string, object?> CoercedVariableValues
    {
        get => CoercedVariableValuesFeature.CoercedVariableValues;
        set => CoercedVariableValuesFeature.CoercedVariableValues = value;
    }

    public IErrorCollector ErrorCollector
    {
        get => ErrorCollectorFeature.ErrorCollector;
        set => ErrorCollectorFeature.ErrorCollector = value;
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
        public IValidatorFeature? Validator;
    }
}
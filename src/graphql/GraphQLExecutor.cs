using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL;

public class GraphQLExecutorBuilder
{
    public GraphQLExecutorBuilder UseOperation(Action<OperationDelegateBuilder> configureOperation)
    {

        return this;
    }
    
    public GraphQLExecutor Build()
    {
        return null;
    }
}

public class GraphQLExecutor
{
    private readonly ILogger<GraphQLExecutor> _logger;
    private readonly OperationDelegate _operationDelegate;
    private readonly ILoggerFactory _loggerFactory;

    public GraphQLExecutor()
    {
        _operationDelegate = new OperationDelegateBuilder()
            .Use(next => (context, ct) => Validate(next, context, ct))
            .Use(next => (context, ct) => ExecuteOperation(next, context, ct))
            .Build();

        _loggerFactory = LoggerFactory.Create(log => log.AddSimpleConsole());
        _logger = _loggerFactory.CreateLogger<GraphQLExecutor>();
    }

    
    public GraphQLExecutor(OperationDelegate operationDelegate, ILoggerFactory loggerFactory)
    {
        _operationDelegate = operationDelegate;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<GraphQLExecutor>();
    }

    public async IAsyncEnumerable<ExecutionResult> Execute(
        ExecutionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var context = BuildQueryContextAsync(options);

        await foreach (var er in _operationDelegate(context, cancellationToken))
            yield return er;
    }

    public async IAsyncEnumerable<ExecutionResult> Validate(
        OperationDelegate next,
        QueryContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var validationResult = Validator.Validate(
            ExecutionRules.All,
            context.Schema,
            context.Document,
            context.CoercedVariableValues
        );

        _logger.ValidationResult(validationResult);

        if (!validationResult.IsValid)
        {
            yield return new ExecutionResult
            {
                Errors = validationResult.Errors.Select(e => e.ToError())
                    .ToList(),
                Extensions = validationResult
                    ?.Extensions
                    ?.ToDictionary(kv => kv.Key, kv => kv.Value)
            };

            yield break;
        }
        
        await foreach (var er in next(context, cancellationToken))
            yield return er;
    }

    public IAsyncEnumerable<ExecutionResult> ExecuteOperation(
        OperationDelegate next,
        QueryContext context,
        CancellationToken cancellationToken)
    {
        _logger.Operation(context.OperationDefinition);
        return context.OperationDefinition.Operation switch
        {
            OperationType.Query => ExecuteQuery(context),
            OperationType.Mutation => ExecuteMutation(context),
            OperationType.Subscription => ExecuteSubscription(context, cancellationToken),
            _ => throw new ArgumentOutOfRangeException()
        };

        static async IAsyncEnumerable<ExecutionResult> ExecuteQuery(QueryContext queryContext)
        {
            var result = await Query.ExecuteQueryAsync(queryContext);

            yield return result;
        }

        static async IAsyncEnumerable<ExecutionResult> ExecuteMutation(QueryContext queryContext)
        {
            var result = await Mutation.ExecuteMutationAsync(queryContext);

            yield return result;
        }

        static async IAsyncEnumerable<ExecutionResult> ExecuteSubscription(
            QueryContext queryContext,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = await Subscription.SubscribeAsync(queryContext, cancellationToken);

            await foreach (var er in result.Source.Reader.ReadAllAsync(cancellationToken)) 
                yield return er;
        }
    }

    public QueryContext BuildQueryContextAsync(ExecutionOptions options)
    {
        var document = options.Document; 
        var operation = Operations.GetOperation(document, options.OperationName);
        var coercedVariableValues = Variables.CoerceVariableValues(
            options.Schema,
            operation,
            options.VariableValues);

        var queryContext = new QueryContext(
            options.FormatError,
            document,
            operation,
            options.Schema,
            coercedVariableValues,
            options.InitialValue,
            new ExtensionsRunner(Array.Empty<IExtensionScope>()));


        return queryContext;
    }
}
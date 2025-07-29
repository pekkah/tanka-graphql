using Microsoft.Extensions.Logging;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL;

internal static class LoggerExtensions
{
    private static readonly Func<ILogger, string, IDisposable?> BeginAction = LoggerMessage
        .DefineScope<string>("Executing '{OperationName}'");

    private static readonly Action<ILogger, bool, Exception> ExecutionResultAction =
        LoggerMessage.Define<bool>(
            LogLevel.Information,
            default,
            "Execution complete. HasErrors: {HasErrors}");

    private static readonly Action<ILogger, bool, string, Exception?> ExecutionResultWithErrorsAction =
        LoggerMessage.Define<bool, string>(
            LogLevel.Information,
            default,
            "Execution complete. HasErrors: {HasErrors}, First: '{error}'");

    private static readonly Action<ILogger, string, string, Exception?> OperationAction = LoggerMessage
        .Define<string, string>(
            LogLevel.Information,
            default,
            "Executing operation '{OperationType} {Name}'"
        );

    private static readonly Action<ILogger, bool, Exception?> ValidateAction = LoggerMessage
        .Define<bool>(
            LogLevel.Information,
            default,
            "Validation requested: '{Validate}'");

    private static readonly Action<ILogger, bool, Exception?> ValidationResultAction =
        LoggerMessage.Define<bool>(
            LogLevel.Information,
            default,
            "Validation IsValid: '{IsValid}'");

    private static readonly Action<ILogger, ValidationResult, Exception?> ValidationResultDebugAction =
        LoggerMessage.Define<ValidationResult>(
            LogLevel.Debug,
            default,
            "Validation result: '{ValidationResult}'");

    internal static IDisposable? Begin(this ILogger logger, string operationName)
    {
        return BeginAction(logger, operationName);
    }

    internal static void Operation(this ILogger logger, OperationDefinition operation)
    {
        OperationAction(
            logger,
            operation.Operation.ToString().ToLowerInvariant(),
            operation.Name ?? string.Empty,
            null);
    }

    internal static void Validate(this ILogger logger, bool validate)
    {
        ValidateAction(
            logger,
            validate,
            null);
    }

    internal static void ValidationResult(this ILogger logger, ValidationResult result)
    {
        ValidationResultAction(logger, result.IsValid, null);

        if (!result.IsValid)
            ValidationResultDebugAction(logger, result, null);
    }

    internal static void ExecutionResult(this ILogger logger, ExecutionResult result)
    {
        if (result.Errors != null && result.Errors.Any())
            ExecutionResultWithErrorsAction(logger, result.Errors != null, result.Errors?[0].Message ?? string.Empty, null);
        else
            ExecutionResultAction(logger, result.Errors != null, null);
    }
}
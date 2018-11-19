using System;
using System.Linq;
using fugu.graphql.validation;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;

namespace fugu.graphql
{
    public static class ExecutorLogger
    {
        private static readonly Func<ILogger, string, IDisposable> BeginAction = LoggerMessage
            .DefineScope<string>("Executing '{OperationName}'");

        private static readonly Action<ILogger, string, string, Exception> OperationAction = LoggerMessage
            .Define<string, string>(
                LogLevel.Information,
                default(EventId),
                "Executing operation '{OperationType} {Name}'"
            );

        private static readonly Action<ILogger, Exception> SchemaNotInitializedAction = LoggerMessage
            .Define(
                LogLevel.Warning,
                default(EventId),
                "Initializing schema. It's recommended that you initialize the schema before execution.");

        private static readonly Action<ILogger, bool, Exception> ValidateAction = LoggerMessage
            .Define<bool>(
                LogLevel.Information,
                default(EventId),
                "Validation requested: '{Validate}'");

        private static readonly Action<ILogger, bool, Exception> ValidationResultAction =
            LoggerMessage.Define<bool>(
                LogLevel.Information,
                default(EventId),
                "Validation IsValid: '{IsValid}'");

        private static readonly Action<ILogger, ValidationResult, Exception> ValidationResultDebugAction =
            LoggerMessage.Define<ValidationResult>(
                LogLevel.Debug,
                default(EventId),
                "Validation result: '{ValidationResult}'");

        private static readonly Action<ILogger, bool, Exception> ExecutionResultAction =
            LoggerMessage.Define<bool>(
                LogLevel.Information,
                default(EventId),
                "Execution complete. HasErrors: {HasErrors}");

        internal static IDisposable Begin(this ILogger logger, string operationName)
        {
            return BeginAction(logger, operationName);
        }

        internal static void SchemaNotInitialized(this ILogger logger)
        {
            SchemaNotInitializedAction(logger, null);
        }

        internal static void Operation(this ILogger logger, GraphQLOperationDefinition operation)
        {
            OperationAction(
                logger,
                operation.Operation.ToString().ToLowerInvariant(),
                operation.Name?.Value, null);
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
            ExecutionResultAction(logger, result.Errors != null, null);
        }
    }
}
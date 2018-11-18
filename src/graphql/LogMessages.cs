using System;
using fugu.graphql.validation;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;

namespace fugu.graphql
{
    public static class ExecutorLogger
    {
        public static readonly EventId EventsOperation = new EventId(2);
        public static readonly EventId EventsSchemaNotInitialized = new EventId(1);

        public static readonly EventId EventsValidate = new EventId(3);

        private static readonly Func<ILogger, string, IDisposable> BeginAction = LoggerMessage
            .DefineScope<string>("Executing '{OperationName}'");

        private static readonly Action<ILogger, string, string, Exception> OperationAction = LoggerMessage
            .Define<string, string>(
                LogLevel.Debug,
                EventsOperation,
                "Operation '{OperationType} {Name}'"
            );

        private static readonly Action<ILogger, Exception> SchemaNotInitializedAction = LoggerMessage
            .Define(
                LogLevel.Warning,
                EventsSchemaNotInitialized,
                "Initializing schema. It's recommended that you initialize the schema before execution.");

        private static readonly Action<ILogger, bool, Exception> ValidateAction = LoggerMessage
            .Define<bool>(
                LogLevel.Information,
                EventsValidate,
                "Validation requested: '{Validate}'");

        private static readonly Action<ILogger, bool, Exception> ValidationResultAction =
            LoggerMessage.Define<bool>(
                LogLevel.Information,
                new EventId(),
                "Validation IsValid: '{IsValid}'");

        private static readonly Action<ILogger, ValidationResult, Exception> ValidationResultDebugAction =
            LoggerMessage.Define<ValidationResult>(
                LogLevel.Debug,
                new EventId(),
                "Validation result: '{ValidationResult}'");

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
            ValidationResultDebugAction(logger, result, null);
        }
    }
}
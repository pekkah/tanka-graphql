using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL
{
    public static partial class Executor
    {
        public static async Task<(QueryContext queryContext, ValidationResult validationResult)>
            BuildQueryContextAsync(
                ExecutionOptions options,
                ExtensionsRunner extensionsRunner,
                ILogger logger)
        {
            await extensionsRunner.BeginParseDocumentAsync();
            var document = options.Document;
            await extensionsRunner.EndParseDocumentAsync(document);

            var operation = Operations.GetOperation(document, options.OperationName);
            logger.Operation(operation);

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
                extensionsRunner);

            logger.Validate(options.Validate != null);
            var validationResult = ValidationResult.Success;
            if (options.Validate != null)
            {
                await extensionsRunner.BeginValidationAsync();
                validationResult = await options.Validate(
                    options.Schema,
                    document,
                    coercedVariableValues);

                logger.ValidationResult(validationResult);
                await extensionsRunner.EndValidationAsync(validationResult);
            }

            return (queryContext, validationResult);
        }
    }
}
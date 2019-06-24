using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using tanka.graphql.execution;
using tanka.graphql.language;
using tanka.graphql.validation;

namespace tanka.graphql
{
    public static partial class Executor
    {
        public static async Task<(QueryContext queryContext, ValidationResult validationResult)>
            BuildQueryContextAsync(ExecutionOptions options,
                Extensions extensions,
                ILogger logger)
        {
            await extensions.BeginParseDocumentAsync();
            var document = options.Document;
            await extensions.EndParseDocumentAsync(document);

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
                extensions);

            logger.Validate(options.Validate != null);
            var validationResult = ValidationResult.Success;
            if (options.Validate != null)
            {
                await extensions.BeginValidationAsync();
                validationResult = await options.Validate(
                    options.Schema,
                    document,
                    coercedVariableValues);

                logger.ValidationResult(validationResult);
                await extensions.EndValidationAsync(validationResult);
            }

            return (queryContext, validationResult);
        }
    }
}
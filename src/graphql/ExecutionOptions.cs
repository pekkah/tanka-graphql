using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using tanka.graphql.type;
using tanka.graphql.validation;

namespace tanka.graphql
{
    /// <summary>
    ///     Execution options
    /// </summary>
    public class ExecutionOptions
    {
        /// <summary>
        ///     Function for formatting <see cref="GraphQLError"/> into <see cref="ExecutionError" />/>
        /// </summary>
        public Func<Exception, ExecutionError> FormatError = DefaultFormatError;

        public Func<ISchema, GraphQLDocument, IReadOnlyDictionary<string, object>, ValueTask<ValidationResult>>
            Validate = (schema, document, variableValues) =>
                DefaultValidate(ExecutionRules.All, schema, document, variableValues);

        /// <summary>
        ///     Schema to execute against
        /// </summary>
        public ISchema Schema { get; set; }

        /// <summary>
        ///     Query, mutation or subscription
        /// </summary>
        public GraphQLDocument Document { get; set; }

        /// <summary>
        ///     Optional operation name
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        ///     Variables values
        /// </summary>
        public Dictionary<string, object> VariableValues { get; set; }

        public object InitialValue { get; set; }

        public ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

        /// <summary>
        ///     Execution extensions
        /// </summary>
        public ICollection<IExtension> Extensions { get; set; } = new List<IExtension>();

        public static ValueTask<ValidationResult> DefaultValidate(
            IEnumerable<CombineRule> rules, 
            ISchema schema,
            GraphQLDocument document,
            IReadOnlyDictionary<string, object> variableValues = null)
        {
            var result = Validator.Validate(
                rules,
                schema,
                document,
                variableValues);

            return new ValueTask<ValidationResult>(result);
        }

        public static ExecutionError DefaultFormatError(Exception exception)
        {
            var message = exception.Message;

            if (exception.InnerException != null)
                message += $" {exception.InnerException.Message}";

            var error = new ExecutionError(message);
            EnrichWithErrorCode(error, exception);
            if (!(exception is GraphQLError graphQLError))
                return error;

            error.Locations = graphQLError.Locations;
            error.Path = graphQLError.Path?.Segments.ToList();
            if (graphQLError.Extensions != null)
                foreach (var extension in graphQLError.Extensions)
                    error.Extend(extension.Key, extension.Value);

            return error;
        }

        public static void EnrichWithErrorCode(ExecutionError error, Exception exception)
        {
            error.Extend("code", exception.GetType().Name
                .Replace("Exception", string.Empty)
                .ToUpperInvariant());
        }
    }
}
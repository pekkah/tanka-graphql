using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL
{
    /// <summary>
    ///     Execution options
    /// </summary>
    public class ExecutionOptions
    {
        public ExecutionOptions()
        {
            FormatError = exception => DefaultFormatError(this, exception);
            Validate = (schema, document, variableValues) =>
                DefaultValidate(ExecutionRules.All, schema, document, variableValues);
        }

        /// <summary>
        ///     Function for formatting <see cref="QueryExecutionException" /> into <see cref="ExecutionError" />/>
        /// </summary>
        public Func<Exception, ExecutionError> FormatError { get; set; }

        public bool IncludeExceptionDetails { get; set; } = false;

        /// <summary>
        ///     Query validator function
        /// </summary>
        public Func<ISchema, ExecutableDocument, IReadOnlyDictionary<string, object>, ValueTask<ValidationResult>>
            Validate { get; set; }

        /// <summary>
        ///     Schema to execute against
        /// </summary>
        public ISchema Schema { get; set; }

        /// <summary>
        ///     Query, mutation or subscription
        /// </summary>
        public ExecutableDocument Document { get; set; }

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
        public ICollection<IExecutorExtension> Extensions { get; set; } = new List<IExecutorExtension>();

        public IExtensionsRunnerFactory ExtensionsRunnerFactory { get; set; } = new ExtensionsRunnerFactory();

        public static ValueTask<ValidationResult> DefaultValidate(
            IEnumerable<CombineRule> rules,
            ISchema schema,
            ExecutableDocument document,
            IReadOnlyDictionary<string, object> variableValues = null)
        {
            var result = Validator.Validate(
                rules,
                schema,
                document,
                variableValues);

            return new ValueTask<ValidationResult>(result);
        }

        public static ExecutionError DefaultFormatError(ExecutionOptions options, Exception exception)
        {
            var rootCause = exception.GetBaseException();
            var message = rootCause.Message;
            var error = new ExecutionError()
            {
                Message = message
            };

            EnrichWithErrorCode(error, rootCause);

            if (options.IncludeExceptionDetails)
                EnrichWithStackTrace(error, rootCause);

            if (!(exception is QueryExecutionException graphQLError))
                return error;

            error.Locations = graphQLError.Nodes?
                .Where(n => !n.Location.Equals(default(Location)))
                .Select(n => n.Location.Value)
                .ToList();
            
            error.Path = graphQLError.Path?.Segments.ToList();

            if (graphQLError.Extensions != null)
                foreach (var extension in graphQLError.Extensions)
                    error.Extend(extension.Key, extension.Value);

            return error;
        }

        public static void EnrichWithErrorCode(ExecutionError error, Exception rootCause)
        {
            var code = rootCause.GetType().Name;

            if (code != "Exception")
                code = code.Replace("Exception", string.Empty);

            error.Extend("code", code.ToUpperInvariant());
        }

        public static void EnrichWithStackTrace(ExecutionError error, Exception exception)
        {
            error.Extend("stacktrace", exception.ToString());
        }
    }
}
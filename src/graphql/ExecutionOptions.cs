using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using tanka.graphql.error;
using tanka.graphql.type;

namespace tanka.graphql
{
    /// <summary>
    ///     Execution options
    /// </summary>
    public class ExecutionOptions
    {
        /// <summary>
        ///     Function for formatting <see cref="GraphQLError" into <see cref="Error" />/>
        /// </summary>
        public Func<Exception, Error> FormatError = DefaultFormatError;

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

        /// <summary>
        ///     Validate <see cref="Document" /> against <see cref="Schema" />
        /// </summary>
        public bool Validate { get; set; } = true;

        public ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

        /// <summary>
        ///     Execution extensions
        /// </summary>
        public ICollection<IExtension> Extensions { get; set; } = new List<IExtension>();

        private static Error DefaultFormatError(Exception exception)
        {
            var message = exception.Message;

            if (exception.InnerException != null) message += $" {exception.InnerException.Message}";

            var error = new Error(message);
            EnrichWithErrorCode(error, exception);
            if (!(exception is GraphQLError graphQLError)) return error;

            error.Locations = graphQLError.Locations;
            error.Path = graphQLError.Path?.Segments.ToList();
            return error;
        }

        private static void EnrichWithErrorCode(Error error, Exception exception)
        {
            error.Extend("code", exception.GetType().Name
                .Replace("Exception", string.Empty)
                .ToUpperInvariant());
        }
    }
}
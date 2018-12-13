using System;
using System.Collections.Generic;
using System.Linq;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace fugu.graphql
{
    /// <summary>
    ///     Execution options
    /// </summary>
    public class ExecutionOptions
    {
        /// <summary>
        ///     Function for formatting <see cref="GraphQLError" into <see cref="Error"/>/>
        /// </summary>
        public Func<GraphQLError, Error> FormatError = DefaultFormatError;

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
        ///     Validate <see cref="Document"/> against <see cref="Schema"/>
        /// </summary>
        public bool Validate { get; set; } = true;

        public ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

        /// <summary>
        ///     Execution extensions
        /// </summary>
        public ICollection<IExtension> Extensions { get; set; } = new List<IExtension>();

        private static Error DefaultFormatError(GraphQLError error)
        {
            var message = error.Message;

            if (error.InnerException != null) message += $" {error.InnerException.Message}";

            return new Error(message)
            {
                Extensions = error.Extensions,
                Locations = error.Locations,
                Path = error.Path?.Segments.ToList()
            };
        }
    }
}
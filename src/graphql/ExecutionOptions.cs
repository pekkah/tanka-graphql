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
    public class ExecutionOptions
    {
        public Func<GraphQLError, Error> FormatError = DefaultFormatError;

        public ISchema Schema { get; set; }

        public GraphQLDocument Document { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> VariableValues { get; set; }

        public object InitialValue { get; set; }

        public bool Validate { get; set; } = true;

        public ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

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
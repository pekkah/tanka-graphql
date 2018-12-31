using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace tanka.graphql.server
{
    internal static class LogMessages
    {
        private static readonly Action<ILogger, string, string, Exception> QueryAction =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                default(EventId),
                "Querying '{OperationName}' with '{Query}'");

        private static readonly Action<ILogger, string, Exception> ExecutedAction =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                default(EventId),
                "Executed '{OperationName}'");

        private static readonly Action<ILogger, string, Exception> SubscribedAction =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                default(EventId),
                "Subscribed '{OperationName}'");

        private static readonly Action<ILogger, string, Exception> UnsubscribedAction =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                default(EventId),
                "Unsubscribed '{OperationName}'");

        internal static void Query(this ILogger logger, QueryRequest query)
        {
            QueryAction(logger, query.OperationName, query.Query, null);
        }

        internal static void Executed(this ILogger logger, string operationName, Dictionary<string, object> variables,
            Dictionary<string, object> extensions)
        {
            ExecutedAction(logger, operationName, null);
        }

        internal static void Subscribed(this ILogger logger, string operationName, Dictionary<string, object> variables,
            Dictionary<string, object> extensions)
        {
            SubscribedAction(logger, operationName, null);
        }

        internal static void Unsubscribed(this ILogger logger, string operationName, Dictionary<string, object> variables,
            Dictionary<string, object> extensions)
        {
            UnsubscribedAction(logger, operationName, null);
        }
    }
}
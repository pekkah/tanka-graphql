using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Microsoft.AspNetCore.SignalR.Client;
using tanka.graphql.language;
using tanka.graphql.requests;

namespace tanka.graphql.links
{
    public static class RemoteLinks
    {
        /// <summary>
        ///     Static data link
        /// </summary>
        /// <param name="result">Execution result</param>
        /// <returns></returns>
        public static ExecutionResultLink Static(ExecutionResult result)
        {
            return async (document, variables, cancellationToken) =>
            {
                var channel = Channel.CreateBounded<ExecutionResult>(1);

                await channel.Writer.WriteAsync(result, cancellationToken);
                channel.Writer.TryComplete();

                return channel;
            };
        }

        /// <summary>
        ///     Http request link using <see cref="HttpLink" />
        /// </summary>
        /// <param name="url"></param>
        /// <param name="createClient"></param>
        /// <param name="transformRequest"></param>
        /// <param name="transformResponse"></param>
        /// <returns></returns>
        public static ExecutionResultLink Http(
            string url,
            Func<HttpClient> createClient = null,
            Func<(GraphQLDocument Document, IDictionary<string, object> Variables, string Url),
                HttpRequestMessage> transformRequest = null,
            Func<HttpResponseMessage, ValueTask<ExecutionResult>> transformResponse = null)
        {
            var link = new HttpLink(url, createClient, transformRequest, transformResponse);
            return link.Execute;
        }

        /// <summary>
        ///     Link to tanka server using SignalR
        ///     <remarks>
        ///         Connection will be managed by the link. It will be started once the query
        ///         begins and stopped once the upstream reader completes.
        ///     </remarks>
        /// </summary>
        /// <param name="connectionBuilderFunc">Must return new connection each call</param>
        /// <returns></returns>
        public static ExecutionResultLink Server(Func<CancellationToken, Task<HubConnection>> connectionBuilderFunc)
        {
            if (connectionBuilderFunc == null) throw new ArgumentNullException(nameof(connectionBuilderFunc));

            return async (document, variables, cancellationToken) =>
            {
                // start connection
                var connection = await connectionBuilderFunc(cancellationToken);
                await connection.StartAsync(cancellationToken);

                // stream query results
                var reader = await connection.StreamQueryAsync(new QueryRequest
                {
                    Query = document.ToGraphQL(),
                    Variables = variables != null ? new Dictionary<string, object>(variables) : null
                }, cancellationToken);

                // stop when done
                var isSubscription = document.Definitions.OfType<GraphQLOperationDefinition>()
                    .Any(op => op.Operation == OperationType.Subscription);

                var _ = Task.Factory.StartNew(async () =>
                {
                    await reader.Completion;
                    await connection.StopAsync(CancellationToken.None);
                }, isSubscription ? TaskCreationOptions.LongRunning : TaskCreationOptions.None);

                // data channel
                return reader;
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
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
        ///     Link for tanka server using SignalR
        /// </summary>
        /// <param name="connection">Hub connection to the server</param>
        /// <returns></returns>
        public static ExecutionResultLink Server(HubConnection connection)
        {
            if (connection.State == HubConnectionState.Disconnected)
                throw new InvalidOperationException("Connection is disconnected. Start it before using.");

            return async (document, variables, cancellationToken) =>
            {
                var channel = await connection.StreamQueryAsync(new QueryRequest
                {
                    Query = document.ToGraphQL(),
                    Variables = variables != null ? new Dictionary<string, object>(variables) : null
                }, cancellationToken);

                return channel;
            };
        }
    }
}
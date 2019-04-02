using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;
using GraphQLParser.AST;

namespace tanka.graphql.links
{
    public static class RemoteLinks
    {
        public static ExecutionResultLink Static(ExecutionResult result)
        {
            return async (document, variables, cancellationToken) =>
            {
                var channel = Channel.CreateUnbounded<ExecutionResult>();

                await channel.Writer.WriteAsync(result, cancellationToken);
                channel.Writer.TryComplete();

                return channel;
            };
        }

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
    }
}
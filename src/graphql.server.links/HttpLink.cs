using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Newtonsoft.Json;
using Tanka.GraphQL.DTOs;
using Tanka.GraphQL.Language;

namespace Tanka.GraphQL.Server.Links
{
    public class HttpLink
    {
        private readonly HttpClient _client;
        private readonly Func<HttpResponseMessage, ValueTask<ExecutionResult>> _transformResponse;
        private readonly string _url;

        private readonly
            Func<(GraphQLDocument Document, IReadOnlyDictionary<string, object> Variables, string Url),
                HttpRequestMessage> _transformRequest;

        public HttpLink(
            string url,
            Func<HttpClient> createClient = null,
            Func<(GraphQLDocument Document, IReadOnlyDictionary<string, object> Variables, string Url),
                HttpRequestMessage> transformRequest = null,
            Func<HttpResponseMessage, ValueTask<ExecutionResult>> transformResponse = null)
        {
            if (createClient == null)
                createClient = () => new HttpClient();

            if (transformRequest == null)
                transformRequest = DefaultTransformRequest;

            if (transformResponse == null)
                transformResponse = DefaultTransformResponse;

            _url = url;
            _client = createClient();
            _transformRequest = transformRequest;
            _transformResponse = transformResponse;
        }

        public static HttpRequestMessage DefaultTransformRequest(
            (GraphQLDocument Document, IReadOnlyDictionary<string, object> Variables, string Url) operation)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, operation.Url);
            var query = new QueryRequest
            {
                Query = operation.Document.ToGraphQL(),
                Variables = operation.Variables?.ToDictionary(kv => kv.Key, kv => kv.Value)
            };
            var json = JsonConvert.SerializeObject(query);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }

        public static async ValueTask<ExecutionResult> DefaultTransformResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ExecutionResult>(json);
        }

        public async ValueTask<ChannelReader<ExecutionResult>> Execute(GraphQLDocument document,
            IReadOnlyDictionary<string, object> variables, CancellationToken cancellationToken)
        {
            var request = _transformRequest((document, variables, _url));

            if (request == null)
                throw new InvalidOperationException(
                    "Executing HttpLink failed. Transform request resulted in null request.");

            var response = await _client.SendAsync(request, cancellationToken);
            var result = await _transformResponse(response);
            var channel = Channel.CreateBounded<ExecutionResult>(1);

            if (result == null)
                throw new InvalidOperationException(
                    "Executing HttpLink failed. Transform response resulted in null result.");

            await channel.Writer.WriteAsync(result, cancellationToken);
            channel.Writer.TryComplete();

            return channel;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Newtonsoft.Json;
using tanka.graphql.language;
using tanka.graphql.requests;

namespace tanka.graphql.links
{

    public class HttpLink
    {
        private readonly string _url;
        private Func<(GraphQLDocument Document, IDictionary<string, object> Variables, string Url), HttpRequestMessage> _transformRequest;
        private Func<HttpResponseMessage, ValueTask<ExecutionResult>> _transformResponse;
        private readonly HttpClient _client;

        public HttpLink(
            string url,
            Func<HttpClient> createClient = null,
            Func<(GraphQLDocument Document, IDictionary<string, object> Variables, string Url),
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

        private HttpRequestMessage DefaultTransformRequest(
            (GraphQLDocument Document, IDictionary<string, object> Variables, string Url) operation)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, operation.Url);
            var query = new QueryRequest()
            {
                Query = operation.Document.ToGraphQL(),
                Variables = operation.Variables != null ? new Dictionary<string, object>(operation.Variables) : null
            };
            var json = JsonConvert.SerializeObject(query);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }

        private async ValueTask<ExecutionResult> DefaultTransformResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ExecutionResult>(json);
        }

        public async ValueTask<ChannelReader<ExecutionResult>> Execute(GraphQLDocument document, IDictionary<string, object> variables, CancellationToken cancellationToken)
        {
            var request = _transformRequest((document, variables, _url));
            var response = await _client.SendAsync(request, cancellationToken);
            var result = await _transformResponse(response);

            var channel = Channel.CreateBounded<ExecutionResult>(1);
            await channel.Writer.WriteAsync(result, cancellationToken);
            channel.Writer.TryComplete();

            return channel;
        }
    }
}
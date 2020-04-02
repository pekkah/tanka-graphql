using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Server.Links.DTOs;

namespace Tanka.GraphQL.Server.Links
{
    public class HttpLink
    {
        private readonly HttpClient _client;
        private readonly Func<HttpResponseMessage, ValueTask<ExecutionResult>> _transformResponse;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new ObjectDictionaryConverter()
            }
        };
        private readonly string _url;

        private readonly
            Func<(ExecutableDocument Document, IReadOnlyDictionary<string, object> Variables, string Url),
                HttpRequestMessage> _transformRequest;

        public HttpLink(
            string url,
            Func<HttpClient> createClient = null,
            Func<(ExecutableDocument Document, IReadOnlyDictionary<string, object> Variables, string Url),
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
            (ExecutableDocument Document, IReadOnlyDictionary<string, object> Variables, string Url) operation)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, operation.Url);
            var query = new QueryRequest
            {
                Query = operation.Document.ToGraphQL(),
                Variables = operation.Variables?.ToDictionary(kv => kv.Key, kv => kv.Value)
            };
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(query, _jsonOptions);
            var json = Encoding.UTF8.GetString(jsonBytes);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }

        public static async ValueTask<ExecutionResult> DefaultTransformResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return JsonSerializer.Deserialize<ExecutionResult>(bytes, _jsonOptions);
        }

        public async ValueTask<ChannelReader<ExecutionResult>> Execute(ExecutableDocument document,
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
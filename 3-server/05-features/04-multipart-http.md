# Multipart HTTP Transport

Tanka GraphQL supports HTTP multipart responses for incremental delivery using `@defer` and `@stream` directives, following the [GraphQL over HTTP specification](https://github.com/graphql/graphql-over-http).

## Content Negotiation

The server automatically detects when clients support multipart responses through the `Accept` header:

### Multipart/Mixed Support
```http
Accept: multipart/mixed, application/json
```

### Defer Specification Support
```http
Accept: application/json; deferSpec=20220824
```

### Fallback Behavior
When multipart is not supported:
```http
Accept: application/json
```
The server returns a single JSON response with all data resolved synchronously.

## Response Format

### Multipart Structure

Multipart responses use the `multipart/mixed` content type with a boundary marker:

```http
HTTP/1.1 200 OK
Content-Type: multipart/mixed; boundary=graphql-response
Transfer-Encoding: chunked
```

### Response Parts

Each part contains a complete JSON response with proper headers:

```http
--graphql-response
Content-Type: application/json; charset=utf-8

{"data":{"user":{"id":"1","name":"John"}},"hasNext":true}

--graphql-response
Content-Type: application/json; charset=utf-8

{"incremental":[{"label":"profile","path":["user"],"data":{"profile":{"email":"john@example.com"}}}],"hasNext":false}

--graphql-response--
```

## HTTP Protocol Details

### Transfer Encoding
- **HTTP/1.1**: Uses `Transfer-Encoding: chunked` for streaming
- **HTTP/2**: Leverages native streaming capabilities
- **Automatic handling**: ASP.NET Core manages the transfer encoding

### Connection Management
- Maintains connection throughout streaming
- Proper connection cleanup on completion or error
- Supports connection timeouts and cancellation

### Error Handling
- Errors can occur in any response part
- Each part can contain its own `errors` array
- Stream continues unless a fatal error occurs

## Performance Characteristics

### Network Efficiency
- **Reduced latency**: First data arrives immediately
- **Bandwidth utilization**: Data flows continuously
- **Connection reuse**: Single HTTP connection for entire operation

### Server Resources
- **Memory usage**: Streaming reduces memory requirements
- **Connection state**: Maintains context during streaming
- **Cancellation support**: Proper cleanup on client disconnect

### Client Considerations
- **Browser support**: Modern browsers handle multipart responses
- **JavaScript parsing**: Requires streaming JSON parser or manual boundary handling
- **React/Vue integration**: Works with streaming data patterns

## Implementation Details

### Boundary Generation
- Uses fixed boundary: `graphql-response`
- Compliant with RFC 1341 multipart format
- Consistent across all responses

### Timing and Logging
The server provides detailed logging for multipart operations:

- **Chunk timing**: Individual part serialization time
- **Data generation timing**: Time to resolve deferred data
- **Total streaming time**: Complete operation duration

### Error Boundaries
- Individual parts can contain errors without stopping the stream
- Fatal errors terminate the stream with proper boundary closure
- Client receives partial data with error information

## Browser Compatibility

### Modern Browsers
All modern browsers support multipart responses:
- **Chrome/Chromium**: Full support
- **Firefox**: Full support
- **Safari**: Full support
- **Edge**: Full support

### Client Libraries
Popular GraphQL clients with multipart support:
- **Apollo Client**: With `@apollo/client/link/subscriptions`
- **Relay**: Native support in recent versions
- **urql**: With streaming exchanges
- **graphql-request**: Manual multipart parsing required

## Configuration

### Server Setup
Multipart support is automatically enabled when incremental delivery directives are registered:

```csharp
services.AddDefaultTankaGraphQLServices();
services.AddIncrementalDeliveryDirectives(); // Enables multipart transport
```

### Custom Headers
The transport automatically sets appropriate headers:
- `Content-Type`: Set to multipart with boundary
- `Transfer-Encoding`: Managed by ASP.NET Core
- `Elapsed`: Custom header with total operation time

## Testing Multipart Responses

### Manual Testing
```bash
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -H "Accept: multipart/mixed" \
  -d '{"query":"{ user { id name ... @defer { profile { email } } } }"}'
```

### Integration Tests

Complete integration tests for multipart HTTP transport can be found in:
- [MultipartHttpTransportTests.cs](https://github.com/pekkah/tanka-graphql/blob/main/tests/GraphQL.Server.Tests/MultipartHttpTransportTests.cs)

## Troubleshooting

### Common Issues

**CORS Configuration**
```csharp
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());
```

**Content-Type Detection**
Ensure clients send proper `Accept` headers for multipart support.

**Streaming Interruption**
Check server logs for connection timeouts or client disconnections.

**Browser Developer Tools**
Multipart responses may appear differently in network tabs - use raw response viewing.
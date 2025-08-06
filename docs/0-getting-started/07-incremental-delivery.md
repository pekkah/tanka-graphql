# Incremental Delivery with @defer and @stream

Incremental delivery allows GraphQL responses to be sent in multiple parts, improving perceived performance by delivering critical data first and streaming less important or slower data afterward.

## Overview

Tanka GraphQL supports two incremental delivery directives:

- **@defer** - Defers expensive fields to be delivered after the initial response
- **@stream** - Streams list items incrementally instead of waiting for the complete list

## Quick Start

### Enable Incremental Delivery

Add incremental delivery support to your services:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultTankaGraphQLServices();
builder.Services.AddIncrementalDeliveryDirectives();

builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchema("MySchema", schema =>
    {
        schema.AddIncrementalDeliveryDirectives();
    });

var app = builder.Build();
app.UseWebSockets();
app.MapTankaGraphQL("/graphql", "MySchema");
app.Run();
```

### Using @defer

Use @defer to mark expensive fields that can be delivered after the initial response:

```graphql
{
  user {
    id
    name
    
    ... @defer(label: "profile") {
      profile {
        bio
        location
      }
    }
  }
}
```

### Using @stream

Use @stream to deliver list items incrementally:

```graphql
{
  products @stream(initialCount: 3) {
    id
    name
    price
  }
}
```

## Content Negotiation

Clients must explicitly request incremental delivery using the Accept header:

```
Accept: multipart/mixed; deferSpec=20220824, application/json
```

## Working Examples

For complete working examples, see the [tutorial tests](https://github.com/pekkah/tanka-graphql/blob/main/tutorials/GraphQL.Tutorials.Getting-Started/DeferStreamTutorials.cs).

## Next Steps

- Learn more about [HTTP multipart transport](../3-server/05-features/04-multipart-http.md)
- Explore [advanced directive usage](../6-executor/11-defer-stream-directives.md)
- Check out the [samples](https://github.com/pekkah/tanka-graphql/tree/main/samples)
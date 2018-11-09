Fugu GraphQL library
=====================================

[![Build Status](https://dev.azure.com/fugu-fw/graphql/_apis/build/status/pekkah.fugu-graphql)](https://dev.azure.com/fugu-fw/graphql/_build/latest?definitionId=1)


## Features

* Execute queries, mutations and subscriptions
* Validation (Dirty port from graphql-dotnet)
* SignalR hub for streaming queries, mutations and subscriptions
* ApolloLink for the provided SignalR hub


## Getting started

### Feeds

Previews: 
NuGet: https://www.myget.org/F/fugu-fw/api/v3/index.json
NPM: https://www.myget.org/F/fugu-fw/npm/

Releases:
(todo)

### Install 

```
dotnet add fugu.graphql
dotnet add fugu.graphql.server
```

## Sample

The included sample does not have '@fugu-fw/fugu-graphql-server-link' as dependency.

To get the sample running use:
```
src/graphql.server.apollo-link$ yarn link
samples/graphql.samples.chat.ui$ yarn link "@fugu-fw/fugu-graphql-server-link"
```

### Usage

(todo) but for now see the samples in the repo

#### Define schema
```
type From {
    userId: ID!
    name: String!
}

type Message {
    id: ID!
    from: From!
    content: String!
    timestamp: String
}

type Query {
    messages: [Message]
}

input InputMessage {
    content: String!
}

type Mutation {
    addMessage(message: InputMessage!): Message!
    editMessage(id: ID!, message: InputMessage!): Message!
}

schema {
    query: Query
    mutation: Mutation
}
```

#### Implement resolvers
```
public class ChatResolvers : ResolverMap
{
    public ChatResolvers(IChatResolverService resolverService)
    {
        this["Query"] = new FieldResolverMap
        {
            {"messages", resolverService.GetMessagesAsync}
        };

        this["Mutation"] = new FieldResolverMap()
        {
            {"addMessage", resolverService.AddMessageAsync},
            {"editMessage", resolverService.EditMessageAsync}
        };

        this["Message"] = new FieldResolverMap()
        {
            {"id", PropertyOf<Message>(m => m.Id)},
            {"from", PropertyOf<Message>(m => m.From)},
            {"content", PropertyOf<Message>(m => m.Content)},
            {"timestamp", PropertyOf<Message>(m => m.Timestamp)}
        };

        this["From"] = new FieldResolverMap()
        {
            {"userId", PropertyOf<From>(f => f.UserId)},
            {"name", PropertyOf<From>(f => f.Name)}
        };
    }
}
```

#### Make executable schema
```
var schema = Sdl.Schema(...);
var resolvers = new ChatResolvers(...);
var executableSchema = SchemaTools.MakeExecutableSchemaWithIntrospection(
                schema,
                resolvers);
```

#### Execute query, mutation
```
using static fugu.graphql.Executor;
using static fugu.graphql.Parser;

[HttpPost]
public async Task<IActionResult> Get([FromBody] OperationRequest request)
{
    var result = await ExecuteAsync(new ExecutionOptions
    {
        Document = ParseDocument(request.Query),
        Schema = _schemas.Chat,
        OperationName = request.OperationName,
        VariableValues = request
                        .Variables
                        .ToVariableDictionary()
    });

    if (result.Errors != null && result.Errors.Any())
        return BadRequest(result);

    return Ok(result);
}
```
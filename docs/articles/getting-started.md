# Getting started

## 1. Create schema

# [IDL](#tab/tabid-1)

[!code[IDL schema](../../samples/graphql.samples.chat.data/idl/schema.graphql)]

[!code-cs[Load IDL schema](../../samples/graphql.samples.chat.data/idl/IdlSchema.cs)]


# [C#](#tab/tabid-2)

[!code-cs[Model schema](../../samples/graphql.samples.chat.data/schema/ModelSchema.cs)]

***


## 2. Create resolvers

Domain service provides the actual functionality
[!code-cs[Domain service](../../samples/graphql.samples.chat.data/Chat.cs)]

Resolvers are used as router between the graphql execution and your domain
[!code-cs[Resolvers](../../samples/graphql.samples.chat.data/ChatResolvers.cs)]


## 3. Make executable with introspection

Connect your schema with the resolvers and add introspection

```cs
var executable = await ExecutableSchema.MakeExecutableSchemaWithIntrospection(
                schema,
                resolvers,
                null, //subscribers
                ResolveFieldConflict); //todo: move conflict resolver to internal
```


## 4. Execute

```cs
using static fugu.graphql.Executor;
using static fugu.graphql.Parser;

var result = await ExecuteAsync(new ExecutionOptions
            {
                Document = ParseDocument("<query>"),
                Schema = executable
            });
```
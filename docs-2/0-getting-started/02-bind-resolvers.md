## Bind resolvers

Resolvers are used to resolve the data for the query. They bind the execution of
GraphQL operation into your logic.

> See also
>
> - [Specification](https://graphql.github.io/graphql-spec/June2018/#sec-Value-Resolution)
> - [Resolvers](xref://exe:01-resolvers.md)

Tanka GraphQL provides few ways of binding resolvers to your schema.

- Bind manually using `SchemaBuilder`
- Bind using resolver maps by using `SchemaBuilder` or `SchemaTools`

### Bind manually using `SchemaBuilder`

[{Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part2_BindResolvers_Manual}]

### Bind using `SchemaTools` or `SchemaBuilder`

This approach uses a map of the types and fields to bind resolvers to fields.

NOTE: Binding is done when the method is called.

[{Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part2_BindResolvers_SchemaBuilder_Maps}]

This is what you would usually do to bind the resolvers and create schema.

[{Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part2_BindResolvers_SchemaTools_Maps}]

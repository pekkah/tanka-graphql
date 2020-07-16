## Input Objects

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Input-Objects)

Input objects are created as instances of `InputObjectType`. 


### Creating input object

[{Tanka.GraphQL.Tests.TypeSystem.InputObjectTypeFacts.Define}]

> When using List or NonNull wrappers make sure that the wrapped type passes `TypeIs.IsInputType`.

### Input Coercion

[{Tanka.GraphQL.Tests.TypeSystem.InputObjectTypeFacts.Input_coercion}]
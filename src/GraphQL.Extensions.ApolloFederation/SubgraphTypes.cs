namespace Tanka.GraphQL.Extensions.ApolloFederation;

public static class SubgraphTypes
{
    public static TypeSystemDocument TypeSystem =>
        @"
scalar _Any
scalar _FieldSet

# a union of all types that use the @key directive
union _Entity

type _Service {
  sdl: String
}

extend type Query {
  # we add this dynamically _entities(representations: [_Any!]!): [_Entity]!
  # see above _service: _Service!
}

directive @external on FIELD_DEFINITION
directive @requires(fields: _FieldSet!) on FIELD_DEFINITION
directive @provides(fields: _FieldSet!) on FIELD_DEFINITION
directive @key(fields: _FieldSet!) repeatable on OBJECT | INTERFACE

directive @extends on OBJECT | INTERFACE
";
}
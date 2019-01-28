## Objects

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Objects)

Objects are created as instances of `ObjectType`. 

### Creating objects

Objects are created by giving it a unique name and list of fields.


#### With scalar field

Type of field defines the return value of field when resolved during execution.

[{tanka.graphql.tests.type.ObjectTypeFacts.With_scalar_field}]

#### With scalar field taking a boolean argument

Fields can take arguments which have a name and type.

[{tanka.graphql.tests.type.ObjectTypeFacts.With_scalar_field_with_argument}]



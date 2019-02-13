## NamedTypeRereference

Supporting cases where the parent `ObjectType` would have a field of its own type would be difficult without providing a way of referencing the type by its name. This reference would be then replaced by the actual type when `Schema` is initialized.

To reference `INamedType` by its name create instance of `NamedTypeReference` giving it the name of the referenced type as parameter.


### Creating NamedTypeReference

Create `ObjectType` with field having the same object as an type using the `NamedTypeReference`. Reference is replaced by the actual type when `Schema` is initialized.

[{tanka.graphql.tests.type.NamedTypeReferenceFacts.Circular_type_reference}]
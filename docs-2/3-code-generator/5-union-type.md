## UnionType

```graphql
type FieldValue1 {
	property: Int!
}

type FieldValue2 {
	method(arg1: Int!): Int!
}

union FieldType = FieldValue1 | FieldValue2
```

Generated
- Marker model interface used to indicate that the type can be be member of an union type
- Controller interface with method for getting the actual Type System type of the union member
- Default implementation of the controller interface


### Model

Marker interface is generated to provide common base interface for members of the union. Each member of the union will implement this interface.

```csharp
public partial interface IFieldType
{
    public string __Typename => "FieldType";
}

public partial class FieldValue1 : IFieldType
{
    //...
}

public partial class FieldValue2 : IFieldType
{
    //...
}
```


### Controller

GraphQL execution requires to know the actual type of the union member during value completion. Controller interface is generated with `IsTypeOf` method to resolve the actual type. This is similar to interfaces.

```csharp
public partial interface IFieldTypeController
{
    INamedType IsTypeOf(IFieldType instance, ISchema schema);
}
```


### Default Controller implementation

Default implementation of the union controller uses the generated `__Typename` property to fetch the named type from the schema.

```csharp
public partial class FieldTypeController : IFieldTypeController
{
    public INamedType IsTypeOf(IFieldType instance, ISchema schema)
    {
        return schema.GetNamedType(instance.__Typename);
    }
}
```
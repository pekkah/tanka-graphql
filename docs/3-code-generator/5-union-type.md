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

```csharp
public partial interface IFieldTypeController
{
    INamedType IsTypeOf(IFieldType instance, ISchema schema);
}
```


### Default Controller implementation

```csharp
public partial class FieldTypeController : IFieldTypeController
{
    public INamedType IsTypeOf(IFieldType instance, ISchema schema)
    {
        return schema.GetNamedType(instance.__Typename);
    }
}
```
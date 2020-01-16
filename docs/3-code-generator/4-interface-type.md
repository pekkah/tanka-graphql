## InterfaceType

```graphql
interface InterfaceType {
	property: Int!
	method(arg1: Int!): Int!
}

type FieldType implements InterfaceType {
	property: Int!
	method(arg1: Int!): Int!
	property2: String!
}
```

Generated
- Model interface with properties for simple fields of the interface
- Controller interface with method for getting the actual Type System type of the object implementing the interface
- Default implementation of the controller interface


### Model

```csharp
public partial interface IInterfaceType
{
    public string __Typename => "InterfaceType";
    public int Property
    {
        get;
    }
}
```


### Controller

```csharp
public partial interface IInterfaceTypeController
{
    INamedType IsTypeOf(IInterfaceType instance, ISchema schema);
}
```


### Default Controller implementation

```csharp
public partial class InterfaceTypeController : IInterfaceTypeController
{
    public INamedType IsTypeOf(IInterfaceType instance, ISchema schema)
    {
        return schema.GetNamedType(instance.__Typename);
    }
}
```
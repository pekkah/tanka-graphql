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

Model class is generated with properties generated according to same rules as for object types.

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

GraphQL execution requires to know the actual type of the interface during value completion. Controller interface is generated with `IsTypeOf` method to resolve the actual type.

```csharp
public partial interface IInterfaceTypeController
{
    INamedType IsTypeOf(IInterfaceType instance, ISchema schema);
}
```


### Default Controller implementation

Default implementation of the interface controller uses the generated `__Typename` property to fetch the named type from the schema.

```csharp
public partial class InterfaceTypeController : IInterfaceTypeController
{
    public INamedType IsTypeOf(IInterfaceType instance, ISchema schema)
    {
        return schema.GetNamedType(instance.__Typename);
    }
}
```
## ObjectType

```graphql
type ObjectType {
	property: Int!
	method(arg1: Int!): Int!
	method2(arg1: Int): Int
}
```

Generated
- Low level interface with resolver method for each field,
- A model class with property for each field of the ObjectType without arguments,
- Abstract base class which implements the generated interface and acts an an 
controller for the generated model.


### Model

Model class represents the type in the schema. 

Property is generated from field of the type when:
- field has no arguments,
- field type is not an union, object or interface,
- or owner type of the field is not an subscription root.

```csharp
public partial class ObjectType
{
    public string __Typename => "ObjectType";
    public int Property
    {
        get;
        set;
    }
}
```

### Controller

Interface is generated with resolver method for each field of the type.

```csharp
public partial interface IObjectTypeController
{
    ValueTask<IResolverResult> Property(IResolverContext context);
    ValueTask<IResolverResult> Method(IResolverContext context);
    ValueTask<IResolverResult> Method2(IResolverContext context);
}
```


### Abstract Controller base class

Default abstract controller base class implementing the interface is generated for the model class.

If field of the type exists as property on the model class then
simple resolver is generated which returns the value of the property
as the resolver result; otherwise abstract method is generated.

> Actual implementation of this class is required with the required logic for fetching the data.

```csharp
public abstract class ObjectTypeControllerBase<T> : IObjectTypeController where T : ObjectType
{
    public virtual async ValueTask<IResolverResult> Property(IResolverContext context)
    {
        var objectValue = (T)context.ObjectValue;
        // if parent field was null this should never run
        if (objectValue == null)
            return Resolve.As(null);
        var resultTask = Property(objectValue, context);
        if (resultTask.IsCompletedSuccessfully)
            return Resolve.As(resultTask.Result);
        var result = await resultTask;
        return Resolve.As(result);
    }

    public virtual ValueTask<int> Property(T objectValue, IResolverContext context)
    {
        return new ValueTask<int>(objectValue.Property);
    }

    public virtual async ValueTask<IResolverResult> Method(IResolverContext context)
    {
        var objectValue = (T)context.ObjectValue;
        // if parent field was null this should never run
        if (objectValue == null)
            return Resolve.As(null);
        var resultTask = Method(objectValue, context.GetArgument<int>("arg1"), context);
        if (resultTask.IsCompletedSuccessfully)
            return Resolve.As(resultTask.Result);
        var result = await resultTask;
        return Resolve.As(result);
    }

    public abstract ValueTask<int> Method(T objectValue, int arg1, IResolverContext context);
    public virtual async ValueTask<IResolverResult> Method2(IResolverContext context)
    {
        var objectValue = (T)context.ObjectValue;
        // if parent field was null this should never run
        if (objectValue == null)
            return Resolve.As(null);
        var resultTask = Method2(objectValue, context.GetArgument<int?>("arg1"), context);
        if (resultTask.IsCompletedSuccessfully)
            return Resolve.As(resultTask.Result);
        var result = await resultTask;
        return Resolve.As(result);
    }

    public abstract ValueTask<int?> Method2(T objectValue, int? arg1, IResolverContext context);
}
```
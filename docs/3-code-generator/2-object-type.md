## ObjectType

Generated
- Low level interface with resolver method for each field,
- A model class with property for each field of the ObjectType without arguments,
- Abstract base class which implements the generated interface and acts an an 
controller for the generated model.

```graphql
type ObjectType 
{
	property: Int!
	method(arg1: Int!): Int!
	method2(arg1: Int): Int
}
```

### Interface

```csharp
public partial interface IObjectTypeController
{
    ValueTask<IResolverResult> Property(IResolverContext context);
    ValueTask<IResolverResult> Method(IResolverContext context);
    ValueTask<IResolverResult> Method2(IResolverContext context);
}
```


### Model

```csharp
public partial class ObjectType
{
    public int Property
    {
        get;
        set;
    }
}
```


### Controller

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
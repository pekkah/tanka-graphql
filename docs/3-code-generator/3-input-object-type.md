## InputObjectType

```graphql
input InputObjectType {
	property1: Int!
	property2: Float!
}
```

Generated
- A Model class implementing the `IReadFromObjectDictionary` interface required to to use the `GetObjectArgument` helper method of `IResolverContext` to read the argument value.


### Model

```csharp
public partial class InputObjectType : IReadFromObjectDictionary
{
    public int Property1
    {
        get;
        set;
    }

    public double Property2
    {
        get;
        set;
    }

    public void Read(IReadOnlyDictionary<string, object> source)
    {
        Property1 = source.GetValue<int>("property1");
        Property2 = source.GetValue<double>("property2");
    }
}
```
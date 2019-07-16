## Query Cost Limit

GraphQL has some unique characteristics which open services
for various types of attacks. Common attach is to overwhelm
the service with resource heavy queries. Common way to counter
this type of attack is to limit the query cost based on complexity.

## Query Cost Analysis

See the detailed explanation and schema configuration in
[Query Cost Analysis](1-execution/07-query-cost-analysis.html).


## Usage with server

Add cost limiting validation rule to options

```csharp
services.AddTankaSchemaOptions()
    .Configure(options =>
    {
        options.ValidationRules = ExecutionRules.All
            .Concat(new[]
            {
                CostAnalyzer.MaxCost(
                    maxCost: 100, 
                    defaultFieldComplexity: 1
                )
            }).ToArray();
    });
```
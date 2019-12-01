## Query Cost Limit

GraphQL has some unique characteristics which open services
for various types of attacks. Common attack is to overwhelm
the service with resource heavy queries. Common way to counter
this type of attack is to limit the query cost based on complexity.

## Query Cost Analysis

See the detailed explanation and schema configuration in
[Query Cost Analysis](5-extensions/5-query-cost-analysis.html).


## Usage with server

Add cost limiting validation rule to options

[{Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Configure_Rules}]
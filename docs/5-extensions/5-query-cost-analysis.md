## Query Cost Analysis

Query cost analysis can be used to limit the complexity of the executed queries and
to protect the service against various attacks (DoS etc). 

Cost of the query is defined by calculating complexity value for each field and
setting a maximum allowed cost. Complexity can be set by setting a default field complexity
which will be used for all fields in a query and/or by using `@cost` directive on the schema.


## Example

Example schema has few fields with and without set complexity.

[{tanka.graphql.tests.analysis.CostFacts.CostFacts}]


### Default field complexity

Default field complexity will be used when `@cost` directive is not present in the field.
        
[{tanka.graphql.tests.analysis.CostFacts.Cost_above_max_cost_with_defaultComplexity}]


### Cost Directive

Complexity of field is set with `@cost` directive.

[{tanka.graphql.tests.analysis.CostFacts.Cost_above_max_cost_with_costDirective}]

In some cases the complexity of the field is related to its arguments. In these cases
value of the argument can be used as mulpliplier for the complexity.

[{tanka.graphql.tests.analysis.CostFacts.Cost_above_max_cost_with_costDirective_and_multiplier}]





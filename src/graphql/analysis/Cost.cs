using System;
using GraphQLParser.AST;
using tanka.graphql.type;
using tanka.graphql.validation;

namespace tanka.graphql.analysis
{
    public static class Analyze
    {
        public static DirectiveType CostDirective = new DirectiveType(
            "cost",
            new[]
            {
                DirectiveLocation.FIELD_DEFINITION
            },
            new Args
            {
                {"complexity", ScalarType.NonNullInt}
            });


        public static CombineRule Cost(
            uint maxCost,
            uint defaultFieldComplexity = 1,
            bool addExtensionData = false,
            Action<(IRuleVisitorContext Context, GraphQLOperationDefinition Operation, uint Cost, uint MaxCost)>
                onCalculated = null
        )
        {
            return (context, rule) =>
            {
                uint cost = 0;
                rule.EnterOperationDefinition += node => { cost = 0; };
                rule.EnterFieldSelection += node =>
                {
                    var fieldDef = context.Tracker.GetFieldDef();

                    if (fieldDef.HasValue)
                    {
                        var field = fieldDef.Value.Field;
                        var costDirective = field.GetDirective("cost");

                        if (costDirective != null)
                        {
                            var complexity = costDirective.GetArgument<int>("complexity");
                            cost += (uint) complexity;
                        }
                        else
                        {
                            cost += defaultFieldComplexity;
                        }
                    }
                };
                rule.LeaveOperationDefinition += node =>
                {
                    onCalculated?.Invoke((context, node, cost, maxCost));

                    if (addExtensionData)
                        context.Extensions.Set("cost", new
                        {
                            Cost = cost,
                            MaxCost = maxCost
                        });


                    if (cost > maxCost)
                        context.Error(
                            "MAX_COST",
                            $"Query cost '{cost}' is too high. Max allowed: '{maxCost}'",
                            node);
                };
            };
        }
    }
}
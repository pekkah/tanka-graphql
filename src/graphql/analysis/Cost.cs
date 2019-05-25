using System.Linq;
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
            new Args()
            {
                {"complexity", ScalarType.NonNullInt}
            });


        public static CombineRule Cost(uint maxCost, uint defaultFieldCost = 1)
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
                            cost += (uint)complexity;
                        }
                        else
                        {
                            cost += defaultFieldCost;
                        } 
                    }
                };
                rule.LeaveOperationDefinition += node =>
                {
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
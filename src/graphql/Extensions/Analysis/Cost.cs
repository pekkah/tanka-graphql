using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Extensions.Analysis;

public static class CostAnalyzer
{
    public static DirectiveDefinition CostDirective =
        @"directive @cost(
                    complexity: Int!
                    multipliers: [String!]
               ) on FIELD_DEFINITION 
            ";

    internal static TypeSystemDocument CostDirectiveAst =
        @"directive @cost(
                    complexity: Int!
                    multipliers: [String!]
               ) on FIELD_DEFINITION 
            ";


    public static CombineRule MaxCost(
        uint maxCost,
        uint defaultFieldComplexity = 1,
        bool addExtensionData = false,
        Action<(IRuleVisitorContext Context, OperationDefinition Operation, uint Cost, uint MaxCost)>?
            onCalculated = null
    )
    {
        return (context, rule) =>
        {
            uint cost = 0;
            rule.EnterOperationDefinition += node => { cost = 0; };
            rule.EnterFieldSelection += node =>
            {
                var field = context.Tracker.FieldDefinition;

                if (field is not null)
                {
                    var complexity = (int)defaultFieldComplexity;

                    if (field.TryGetDirective("cost", out var costDirective))
                    {
                        if (costDirective.TryGetArgument("complexity", out var complexityArg))
                            complexity = (int?)Values.CoerceValue(context.Schema, complexityArg?.Value, "Int!") ?? 0;

                        costDirective.TryGetArgument("multipliers", out var multipliersArg);
                        if (Values.CoerceValue(context.Schema, multipliersArg?.Value, "[String!]") is
                            IEnumerable<object> multipliers)
                            foreach (var multiplier in multipliers.Select(o => o.ToString()))
                            {
                                var multiplierName = multiplier;
                                field.TryGetArgument(multiplierName, out var multiplierArgDef);

                                if (multiplierArgDef == null)
                                    continue;

                                var multiplierArg =
                                    node.Arguments?.SingleOrDefault(a => a.Name == multiplierName);

                                if (multiplierArg == null)
                                    continue;

                                var multiplierValue = (int?)ArgumentCoercion.CoerceArgumentValue(
                                    context.Schema,
                                    context.VariableValues,
                                    multiplierName,
                                    multiplierArgDef,
                                    multiplierArg) ?? 1;

                                complexity *= multiplierValue;
                            }
                    }

                    cost += (uint)complexity;
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
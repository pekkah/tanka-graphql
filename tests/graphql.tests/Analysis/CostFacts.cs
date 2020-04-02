using System;
using System.Collections.Generic;

using Tanka.GraphQL.Extensions.Analysis;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Analysis
{
    public class CostFacts
    {
        public CostFacts()
        {
            var sdl =
                @"
                schema {
                    query: Query
                }

                type Query {
                    default: Int
                    withCost: Int @cost(complexity: 1)
                    withMultiplier(count: Int!): Int @cost(complexity: 2, multipliers: [""count""])
                }
                ";

            Schema = new SchemaBuilder()
                .Include(CostAnalyzer.CostDirective)
                .Sdl(sdl)
                .Build();
        }

        public ISchema Schema { get; }

        private ValidationResult Validate(
            ExecutableDocument document,
            CombineRule rule,
            Dictionary<string, object> variables = null)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            return Validator.Validate(
                new[] {rule},
                Schema,
                document,
                variables);
        }

        [Fact]
        public void Cost_above_max_cost_with_costDirective()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                    withCost
                }");

            /* When */
            var result = Validate(
                document,
                CostAnalyzer.MaxCost(
                    maxCost: 0, 
                    defaultFieldComplexity: 0)
                );

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == "MAX_COST");
        }

        [Fact]
        public void Cost_above_max_cost_with_costDirective_and_multiplier()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                    withMultiplier(count: 5)
                }");

            /* When */
            var result = Validate(
                document,
                CostAnalyzer.MaxCost(
                    maxCost: 5, 
                    defaultFieldComplexity: 0)
                );

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == "MAX_COST");
        }

        [Fact]
        public void Cost_above_max_cost_with_defaultComplexity()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                    default
                }");

            /* When */
            var result = Validate(
                document,
                CostAnalyzer.MaxCost(
                    maxCost: 0,
                    defaultFieldComplexity: 1)
                );

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == "MAX_COST");
        }

        [Fact]
        public void Cost_below_max_cost_with_defaultComplexity()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                    default
                }");

            /* When */
            var result = Validate(
                document,
                CostAnalyzer.MaxCost(
                    maxCost: 1,
                    defaultFieldComplexity: 1)
                );

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Cost_below_max_cost_with_with_costDirective()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                    withCost
                }");

            /* When */
            var result = Validate(
                document,
                CostAnalyzer.MaxCost(
                    maxCost: 1, 
                    defaultFieldComplexity: 0)
                );

            /* Then */
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Cost_below_max_cost_with_with_costDirective_and_multiplier()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                    withMultiplier
                }");

            /* When */
            var result = Validate(
                document,
                CostAnalyzer.MaxCost(
                    maxCost: 3, 
                    defaultFieldComplexity: 0)
                );

            /* Then */
            Assert.True(result.IsValid);
        }
    }
}
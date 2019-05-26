using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using tanka.graphql.analysis;
using tanka.graphql.sdl;
using tanka.graphql.type;
using tanka.graphql.validation;
using Xunit;

namespace tanka.graphql.tests.analysis
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
                .IncludeDirective(Analyze.CostDirective)
                .Sdl(sdl)
                .Build();
        }

        public ISchema Schema { get; }

        private ValidationResult Validate(
            GraphQLDocument document,
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
                Analyze.Cost(0, 0));

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
                Analyze.Cost(5, 0));

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == "MAX_COST");
        }

        [Fact]
        public void Cost_above_max_cost_with_defaultCost()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                    default
                }");

            /* When */
            var result = Validate(
                document,
                Analyze.Cost(0));

            /* Then */
            Assert.False(result.IsValid);
            Assert.Single(
                result.Errors,
                error => error.Code == "MAX_COST");
        }

        [Fact]
        public void Cost_below_max_cost_with_defaultCost()
        {
            /* Given */
            var document = Parser.ParseDocument(
                @"{
                    default
                }");

            /* When */
            var result = Validate(
                document,
                Analyze.Cost(1));

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
                Analyze.Cost(1, 0));

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
                Analyze.Cost(3, 0));

            /* Then */
            Assert.True(result.IsValid);
        }
    }
}
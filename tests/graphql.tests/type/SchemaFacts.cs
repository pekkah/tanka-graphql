﻿using System;
using tanka.graphql.type;
using tanka.graphql.typeSystem;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class SchemaFacts
    {
        [Fact]
        public void Build_types()
        {
            /* Given */
            var builder = new SchemaBuilder();

            builder.Object("Object1", out var object1)
                .Field(object1, "field1", ScalarType.Float);

            builder.Query(out var query)
                .Field(query, "field1", object1);

            /* When */
            var sut = builder.Build();

            /* Then */
            var types = sut.QueryTypes<INamedType>();

            Assert.Contains(types, t => t.Name == "Query");
            Assert.Contains(types, t => t.Name == "Object1");
        }

        [Fact]
        public void Require_Query()
        {
            /* Given */
            var builder = new SchemaBuilder();

            /* When */
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Build());

            /* Then */
            Assert.Equal("types", exception.ParamName);
        }

        [Fact]
        public void Set_Mutation()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Query(out _);
            builder.Mutation(out var mutation);

            /* When */
            var sut = builder.Build();

            /* Then */
            Assert.Equal(mutation, sut.Mutation);
        }

        [Fact]
        public void Set_Query()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Query(out var query);

            /* When */
            var sut = builder.Build();

            /* Then */
            Assert.Equal(query, sut.Query);
        }

        [Fact]
        public void Set_Subscription()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Query(out _);
            builder.Mutation(out _);
            builder.Subscription(out var subscription);

            /* When */
            var sut = builder.Build();

            /* Then */
            Assert.Equal(subscription, sut.Subscription);
        }
    }
}
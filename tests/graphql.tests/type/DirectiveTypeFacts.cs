using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.tools;
using tanka.graphql.type;
using tanka.graphql.typeSystem;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class DirectiveTypeFacts
    {
        [Fact]
        public async Task Authorize_field_directive()
        {
            /* Given */
            var authorizeType = new DirectiveType(
                "authorize",
                new[]
                {
                    DirectiveLocation.FIELD_DEFINITION
                });

            var builder = new SchemaBuilder();
            builder.IncludeDirective(authorizeType);

            builder.Query(out var query)
                .Field(query, "requiresAuthorize", ScalarType.NonNullString,
                    directives: new [] {
                        authorizeType.CreateInstance()

                    });

            var resolvers = new ResolverMap
            {
                {
                    query.Name, new FieldResolverMap
                    {
                        {"requiresAuthorize", context => Task.FromResult(Resolve.As("Hello World!"))}
                    }
                }
            };

            Func<bool> authorize = () => true;

            /* When */
            var schema = await SchemaTools.MakeExecutableSchemaAsync(
                builder.Build(),
                resolvers,
                visitors: new SchemaVisitorFactory[]
                {
                    (s, _1, _2) => new Authorize(s, authorize)
                });

            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = Parser.ParseDocument(@"{ requiresAuthorize }"),
                Schema = schema
            });

            /* Then */
            Assert.Equal("Hello World! authorize: True", result.Data["requiresAuthorize"]);
        }
    }

    public class Authorize : SchemaVisitorBase
    {
        private readonly Func<bool> _authorize;

        public Authorize(ISchema schema, Func<bool> authorize) : base(schema)
        {
            _authorize = authorize;
        }

        protected override Task VisitObjectFieldAsync(ObjectType objectType,
            KeyValuePair<string, IField> objectTypeField)
        {
            var authorizeDirective = objectTypeField.Value.GetDirective("authorize");

            if (authorizeDirective != null)
            {
                var field = objectTypeField.Value;
                var originalResolver = field.Resolve;

                field.Resolve = async context =>
                {
                    var result = await originalResolver(context);
                    var value = $"{result.Value} authorize: {_authorize()}";

                    return Resolve.As(value);
                };
            }

            return base.VisitObjectFieldAsync(objectType, objectTypeField);
        }
    }
}
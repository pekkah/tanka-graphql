using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class DirectiveTypeFacts
    {
        [Fact(Skip = "Revisit with SchemaBuilder syntax")]
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
                .Connections(connect => connect
                    .Field(query, "requiresAuthorize", ScalarType.NonNullString,
                        directives: new[]
                        {
                            authorizeType.CreateInstance()
                        }));

            var resolvers = new ResolverMap
            {
                {
                    query.Name, new FieldResolverMap
                    {
                        {"requiresAuthorize", context => new ValueTask<IResolveResult>(Resolve.As("Hello World!"))}
                    }
                }
            };

            Func<bool> authorize = () => true;

            /* When */
            var schema = SchemaTools.MakeExecutableSchema(
                builder.Build(),
                resolvers);

            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = Parser.ParseDocument(@"{ requiresAuthorize }"),
                Schema = schema
            });

            /* Then */
            Assert.Equal("Hello World! authorize: True", result.Data["requiresAuthorize"]);
        }
    }
}
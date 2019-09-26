using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Tanka.GraphQL.Directives;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class DirectiveTypeFacts
    {
        public static CreateDirectiveVisitor AuthorizeVisitor(Func<int, ClaimsPrincipal> fetchUser)
        {
            return builder => new DirectiveVisitor
            {
                FieldDefinition = (directive, fieldDefinition) =>
                {
                    return fieldDefinition.WithResolver(resolver => resolver.Use((context, next) =>
                    {
                        var requiredRole = directive.GetArgument<string>("role");
                        var user = fetchUser(42);
                        
                        if (!user.HasClaim("role", requiredRole))
                            throw new Exception(
                                "requires admin role. "
                            );

                        return next(context);
                    }).Run(fieldDefinition.Resolver));
                }
            };
        }

        [Fact]
        public async Task Authorize_field_directive()
        {
            /* Given */
            var authorizeType = new DirectiveType(
                "authorize",
                new[]
                {
                    DirectiveLocation.FIELD_DEFINITION
                },
                new Args
                {
                    {"role", ScalarType.NonNullString, "user", "Required role"}
                });

            var builder = new SchemaBuilder();
            builder.Include(authorizeType);

            builder.Query(out var query)
                .Connections(connect => connect
                    .Field(query, "requiresAdmin", ScalarType.String,
                        directives: new[]
                        {
                            authorizeType.CreateInstance(new Dictionary<string, object>
                            {
                                // this will override the default value of the DirectiveType
                                ["role"] = "admin"
                            })
                        })
                    .Field(query, "requiresUser", ScalarType.String,
                        directives: new[]
                        {
                            // this will use defaultValue from DirectiveType
                            authorizeType.CreateInstance()
                        }));

            var resolvers = new ObjectTypeMap
            {
                {
                    query.Name, new FieldResolversMap
                    {
                        {"requiresAdmin", context => new ValueTask<IResolveResult>(Resolve.As("Hello Admin!"))},
                        {"requiresUser", context => new ValueTask<IResolveResult>(Resolve.As("Hello User!"))}
                    }
                }
            };

            // mock user and user store
            var user = new ClaimsPrincipal(new ClaimsIdentity(new []
            {
                new Claim("role", "user"),
            }));

            ClaimsPrincipal FetchUser(int id) => user;

            /* When */
            var schema = SchemaTools.MakeExecutableSchema(
                builder,
                resolvers,
                directives: new Dictionary<string, CreateDirectiveVisitor>
                {
                    // register directive visitor to be used when authorizeType.Name present
                    [authorizeType.Name] = AuthorizeVisitor(FetchUser)
                });

            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = Parser.ParseDocument(@"{ requiresAdmin requiresUser }"),
                Schema = schema
            });

            /* Then */
            result.ShouldMatchJson(
                @"
                  {
                  ""data"": {
                    ""requiresAdmin"": null,
                    ""requiresUser"": ""Hello User!""
                  },
                  ""errors"": [
                    {
                      ""message"": ""requires admin role. "",
                      ""locations"": [
                        {
                          ""end"": 28,
                          ""start"": 2
                        }
                      ],
                      ""path"": [
                        ""requiresAdmin""
                      ],
                      ""extensions"": {
                        ""code"": ""EXCEPTION""
                      }
                    }
                  ]
                }
                ");
        }

        [Fact]
        public async Task Authorize_field_directive_sdl()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .Sdl(Parser.ParseDocument(@"
                directive @authorize(
                    role: String =""user""
                ) on FIELD_DEFINITION

                type Query {
                    requiresAdmin: String @authorize(role:""admin"")
                    requiresUser: String @authorize
                }

                schema {
                    query: Query
                }
                "));

            var resolvers = new ObjectTypeMap
            {
                {
                    "Query", new FieldResolversMap
                    {
                        {"requiresAdmin", context => new ValueTask<IResolveResult>(Resolve.As("Hello Admin!"))},
                        {"requiresUser", context => new ValueTask<IResolveResult>(Resolve.As("Hello User!"))}
                    }
                }
            };

            // mock user and user store
            var user = new ClaimsPrincipal(new ClaimsIdentity(new []
            {
                new Claim("role", "user"),
            }));

            ClaimsPrincipal FetchUser(int id) => user;

            /* When */
            var schema = SchemaTools.MakeExecutableSchema(
                builder,
                resolvers,
                directives: new Dictionary<string, CreateDirectiveVisitor>
                {
                    // register directive visitor to be used when authorizeType.Name present
                    ["authorize"] = AuthorizeVisitor(FetchUser)
                });

            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = Parser.ParseDocument(@"{ requiresAdmin requiresUser }"),
                Schema = schema
            });

            /* Then */
            result.ShouldMatchJson(@"
                  {
                  ""data"": {
                    ""requiresAdmin"": null,
                    ""requiresUser"": ""Hello User!""
                  },
                  ""errors"": [
                    {
                      ""message"": ""requires admin role. "",
                      ""locations"": [
                        {
                          ""end"": 28,
                          ""start"": 2
                        }
                      ],
                      ""path"": [
                        ""requiresAdmin""
                      ],
                      ""extensions"": {
                        ""code"": ""EXCEPTION""
                      }
                    }
                  ]
                }
                ");
        }
    }
}
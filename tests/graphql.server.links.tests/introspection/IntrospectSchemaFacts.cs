using System.Threading.Tasks;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Xunit;
using static Tanka.GraphQL.Parser;

// ReSharper disable InconsistentNaming

namespace Tanka.GraphQL.Server.Links.Tests.Introspection
{
    public class IntrospectSchemaFacts
    {
        public IntrospectSchemaFacts()
        {
            var builder = new SchemaBuilder();

            builder.Interface("Interface", out var interface1,
                    "Description")
                .Connections(connect => connect
                    .Field(interface1, ScalarFieldName, ScalarType.Int));

            builder.Object(ObjectTypeName, out var type1,
                    "Description",
                    new[] {interface1})
                .Connections(connect => connect
                    .Field(type1, ScalarFieldName, ScalarType.NonNullInt,
                        "Description",
                        args: args => args.Arg("arg1", ScalarType.Float, 1d, "Description")));

            builder.Object($"{ObjectTypeName}2", out var type2,
                    "Description")
                .Connections(connect => connect
                    .Field(type2, ScalarFieldName, new List(ScalarType.Int)));


            var union = new UnionType(
                "Union",
                new[] {type1, type2},
                "Description");

            builder.Include(union);

            var enum1 = new EnumType(
                "Enum",
                new EnumValues
                {
                    {"value1", "Description"},
                    {"value2", "Description", null, "Deprecated"}
                },
                "Description");

            builder.Include(enum1);

            builder.InputObject("InputObject", out var inputObject,
                    "Description")
                .Connections(connect => connect
                    .InputField(inputObject, "field1", ScalarType.Boolean, true, "Description"));

            builder.Query(out var query)
                .Connections(connect => connect
                    .Field(query, "object", type1)
                    .Field(query, "union", union)
                    .Field(query, "enum", enum1)
                    .Field(query, "listOfObjects", new List(type2))
                    .Field(query, "nonNullObject", new NonNull(type1))
                    .Field(query, "inputObjectArg", ScalarType.NonNullBoolean,
                        args: args => args.Arg("arg1", inputObject, default, "With inputObject arg")));

            builder.Mutation(out var mutation);
            builder.Subscription(out var subscription);

            var sourceSchema = builder.Build();
            _introspectionSchema = Introspect.Schema(sourceSchema);
        }

        private readonly ISchema _introspectionSchema;

        public const string ObjectTypeName = "Object";
        public const string ScalarFieldName = "int";

        private async Task<ExecutionResult> QueryAsync(string query)
        {
            return await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _introspectionSchema,
                Document = ParseDocument(query)
            });
        }

        [Fact]
        public async Task Schema_directives()
        {
            /* Given */
            var query = @"{ 
                            __schema {
                                directives { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__schema"": {
                      ""directives"": [
                        {
                          ""name"": ""include""
                        },
                        {
                          ""name"": ""skip""
                        }
                      ]
                    }
                  }
                }");
        }

        [Fact]
        public async Task Schema_root_types()
        {
            /* Given */
            var query = @"{ 
                            __schema {
                                queryType { name }
                                mutationType { name }
                                subscriptionType { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__schema"": {
                      ""queryType"": {
                        ""name"": ""Query""
                      },
                      ""mutationType"": {
                        ""name"": ""Mutation""
                      },
                      ""subscriptionType"": {
                        ""name"": ""Subscription""
                      }
                    }
                  }
                }");
        }

        [Fact]
        public async Task Schema_types()
        {
            /* Given */
            var query = @"{ 
                            __schema {
                                types { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__schema"": {
                      ""types"": [
                        {
                          ""name"": ""String""
                        },
                        {
                          ""name"": ""Int""
                        },
                        {
                          ""name"": ""Float""
                        },
                        {
                          ""name"": ""Boolean""
                        },
                        {
                          ""name"": ""ID""
                        },
                        {
                          ""name"": ""Interface""
                        },
                        {
                          ""name"": ""Object""
                        },
                        {
                          ""name"": ""Object2""
                        },
                        {
                          ""name"": ""Union""
                        },
                        {
                          ""name"": ""Enum""
                        },
                        {
                          ""name"": ""InputObject""
                        },
                        {
                          ""name"": ""Query""
                        },
                        {
                          ""name"": ""Mutation""
                        },
                        {
                          ""name"": ""Subscription""
                        }
                      ]
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_DirectiveType()
        {
            /* Given */
            var query = @"{ 
                            __schema {
                                directives {
                                    name
                                    description
                                    locations
                                    args { name }
                                }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__schema"": {
                      ""directives"": [
                        {
                          ""locations"": [
                            ""FIELD"",
                            ""FRAGMENT_SPREAD"",
                            ""INLINE_FRAGMENT""
                          ],
                          ""description"": """",
                          ""name"": ""include"",
                          ""args"": [
                            {
                              ""name"": ""if""
                            }
                          ]
                        },
                        {
                          ""locations"": [
                            ""FIELD"",
                            ""FRAGMENT_SPREAD"",
                            ""INLINE_FRAGMENT""
                          ],
                          ""description"": """",
                          ""name"": ""skip"",
                          ""args"": [
                            {
                              ""name"": ""if""
                            }
                          ]
                        }
                      ]
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_EnumType()
        {
            /* Given */
            //todo(pekka): separate enumValues testing to own test
            var query = @"{ 
                            __type(name: ""Enum"") {
                                kind
                                name
                                description
                                enumValues { 
                                  name
                                  description
                                  isDeprecated
                                  deprecationReason
                                }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""description"": ""Description"",
                      ""name"": ""Enum"",
                      ""enumValues"": [
                        {
                          ""description"": ""Description"",
                          ""name"": ""VALUE1"",
                          ""isDeprecated"": false,
                          ""deprecationReason"": null
                        }
                      ],
                      ""kind"": ""ENUM""
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_EnumType_include_deprecated()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""Enum"") {
                                kind
                                name
                                description
                                enumValues(includeDeprecated: true) { 
                                  name
                                  description
                                  isDeprecated
                                  deprecationReason
                                }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""name"": ""Enum"",
                      ""enumValues"": [
                        {
                          ""isDeprecated"": false,
                          ""name"": ""VALUE1"",
                          ""description"": ""Description"",
                          ""deprecationReason"": null
                        },
                        {
                          ""isDeprecated"": true,
                          ""name"": ""VALUE2"",
                          ""description"": ""Description"",
                          ""deprecationReason"": ""Deprecated""
                        }
                      ],
                      ""description"": ""Description"",
                      ""kind"": ""ENUM""
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_InputObjectType()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""InputObject"") {
                                kind
                                name
                                description
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""name"": ""InputObject"",
                      ""description"": ""Description"",
                      ""kind"": ""INPUT_OBJECT""
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_InputObjectType_fields()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""InputObject"") {
                                inputFields {
                                    name
                                    description
                                    type { name kind }
                                    defaultValue
                                }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""inputFields"": [
                        {
                          ""description"": ""Description"",
                          ""name"": ""field1"",
                          ""defaultValue"": ""True"",
                          ""type"": {
                            ""kind"": ""SCALAR"",
                            ""name"": ""Boolean""
                          }
                        }
                      ]
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_InterfaceType()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""Interface"") {
                                kind
                                name
                                description
                                fields { name }
                                possibleTypes { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""possibleTypes"": [
                        {
                          ""name"": ""Object""
                        }
                      ],
                      ""name"": ""Interface"",
                      ""kind"": ""INTERFACE"",
                      ""fields"": [
                        {
                          ""name"": ""int""
                        }
                      ],
                      ""description"": ""Description""
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_ObjectType()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""Object"") {
                                kind
                                name
                                description
                                fields { name }
                                interfaces { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""interfaces"": [
                        {
                          ""name"": ""Interface""
                        }
                      ],
                      ""fields"": [
                        {
                          ""name"": ""int""
                        }
                      ],
                      ""kind"": ""OBJECT"",
                      ""description"": ""Description"",
                      ""name"": ""Object""
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_ObjectType_fields()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""Object"") {
                                fields { 
                                    name 
                                    description
                                    isDeprecated
                                    deprecationReason
                                    type { name kind ofType { name } }
                                    args { name }
                                }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""fields"": [
                        {
                          ""description"": ""Description"",
                          ""name"": ""int"",
                          ""isDeprecated"": false,
                          ""args"": [
                            {
                              ""name"": ""arg1""
                            }
                          ],
                          ""deprecationReason"": null,
                          ""type"": {
                            ""name"": null,
                            ""kind"": ""NON_NULL"",
                            ""ofType"": {
                              ""name"": ""Int""
                            }
                          }
                        }
                      ]
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_ObjectType_fields_args()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""Object"") {
                                fields { 
                                    args { 
                                        name 
                                        description
                                        type { name }
                                        defaultValue
                                    }
                                }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""fields"": [
                        {
                          ""args"": [
                            {
                              ""name"": ""arg1"",
                              ""defaultValue"": ""1"",
                              ""type"": {
                                ""name"": ""Float""
                              },
                              ""description"": ""Description""
                            }
                          ]
                        }
                      ]
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_ScalarType()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""Int"") {
                                kind
                                name
                                description
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""name"": ""Int"",
                      ""description"": ""The `Int` scalar type represents non-fractional signed whole numeric values"",
                      ""kind"": ""SCALAR""
                    }
                  }
                }");
        }

        [Fact]
        public async Task Type_UnionType()
        {
            /* Given */
            var query = @"{ 
                            __type(name: ""Union"") {
                                kind
                                name
                                description
                                possibleTypes { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__type"": {
                      ""kind"": ""UNION"",
                      ""description"": ""Description"",
                      ""possibleTypes"": [
                        {
                          ""name"": ""Object""
                        },
                        {
                          ""name"": ""Object2""
                        }
                      ],
                      ""name"": ""Union""
                    }
                  }
                }");
        }
    }
}
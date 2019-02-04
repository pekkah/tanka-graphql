using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.introspection;
using tanka.graphql.tests.data;
using tanka.graphql.type;
using Xunit;
using static tanka.graphql.Parser;

// ReSharper disable InconsistentNaming

namespace tanka.graphql.tests.introspection
{
    public class IntrospectSchemaFacts
    {
        public IntrospectSchemaFacts()
        {
            var interface1 = new InterfaceType(
                "Interface",
                new Fields
                {
                    {ScalarFieldName, ScalarType.Int}
                },
                new Meta("Description"));

            var type1 = new ObjectType(
                ObjectTypeName,
                new Fields
                {
                    {ScalarFieldName, ScalarType.NonNullInt, new Args()
                    {
                        {"arg1", ScalarType.Float, 1d, new Meta("Description")}
                    }, new Meta("Description")}
                },
                new Meta("Description"),
                new[] {interface1});

            var type2 = new ObjectType(
                $"{ObjectTypeName}2",
                new Fields
                {
                    {ScalarFieldName, new List(ScalarType.Int)}
                },
                new Meta("Description"));

            var union = new UnionType(
                "Union",
                new[] {type1, type2},
                new Meta("Description"));

            var enum1 = new EnumType(
                "Enum",
                new EnumValues()
                {
                    {"value1", "Description"},
                    {"value2", "Description", "Deprecated"}
                },
                new Meta("Description"));

            var inputObject = new InputObjectType(
                "InputObject",
                new InputFields()
                {
                    {"field1", ScalarType.Boolean, true, new Meta("Description")}
                }, new Meta("Description"));

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    {"object", type1},
                    {"union", union},
                    {"enum", enum1},
                    {"listOfObject", new List(type2)},
                    {"nonNullObject", new NonNull(type1)},
                    {"inputObjectArg", ScalarType.NonNullBoolean, new Args()
                    {
                        {"arg1", inputObject}
                    }, new Meta("With inputObject arg")}
                });

            var mutation = new ObjectType(
                "Mutation",
                new Fields());

            var subscription = new ObjectType(
                "Subscription",
                new Fields());

            _sourceSchema = new Schema(query, mutation, subscription);
            _introspectionSchema = Introspect.SchemaAsync(_sourceSchema)
                .GetAwaiter()
                .GetResult();
        }

        private readonly Schema _sourceSchema;
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
                          ""name"": ""InputObject""
                        },
                        {
                          ""name"": ""Boolean""
                        },
                        {
                          ""name"": ""Object2""
                        },
                        {
                          ""name"": ""Enum""
                        },
                        {
                          ""name"": ""Union""
                        },
                        {
                          ""name"": ""Float""
                        },
                        {
                          ""name"": ""Int""
                        },
                        {
                          ""name"": ""Interface""
                        },
                        {
                          ""name"": ""Object""
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
                          ""type"": {
                            ""kind"": ""SCALAR"",
                            ""name"": ""Boolean""
                          },
                          ""name"": ""field1"",
                          ""defaultValue"": ""True"",
                          ""description"": ""Description""
                        }
                      ]
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
                              ""type"": {
                                ""name"": ""Float""
                              },
                              ""name"": ""arg1"",
                              ""defaultValue"": ""1"",
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
using System.Threading.Tasks;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.TypeSystem;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Tanka.GraphQL.Tests.Introspection
{
    public class IntrospectSchemaFacts
    {
        public IntrospectSchemaFacts()
        {
            var builder = new SchemaBuilder();
            builder.Add(@"

""""""Description""""""
interface Interface 
{
    int: Int    
}

""""""Description""""""
type Object implements Interface
{
    int: Int
    """"""Description""""""
    nonNullInt(arg1: Float = 1): Int!
}

type Object2 
{
    int: [Int]
}

""""""Description""""""
union Union = Object | Object2

""""""Description""""""
enum Enum {
    """"""Description""""""
    VALUE1
    
    """"""Description""""""
    VALUE2 @deprecated(reason: ""reason"")
}

""""""Description""""""
input InputObject 
{
    """"""Description""""""
    field1: Boolean = true
}

type Query {
    object: Object
    union: Union
    enum: Enum
    listOfObjects: [Object2]
    nonNullObject: Object1!

    """"""Description""""""
    inputObjectArg(arg1: InputObject): Boolean!
}

type Mutation {}
type Subscription {}
");


            _introspectionSchema = builder.Build(new SchemaBuildOptions()).Result;
        }

        private readonly ISchema _introspectionSchema;

        public const string ObjectTypeName = "Object";
        public const string ScalarFieldName = "int";

        private async Task<ExecutionResult> QueryAsync(string query)
        {
            return await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _introspectionSchema,
                Document = query,
                IncludeExceptionDetails = true
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
          ""name"": ""deprecated""
        },
        {
          ""name"": ""include""
        },
        {
          ""name"": ""skip""
        },
        {
          ""name"": ""specifiedBy""
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
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
          ""name"": ""Enum""
        },
        {
          ""name"": ""InputObject""
        },
        {
          ""name"": ""Interface""
        },
        {
          ""name"": ""Mutation""
        },
        {
          ""name"": ""Object""
        },
        {
          ""name"": ""Object2""
        },
        {
          ""name"": ""Query""
        },
        {
          ""name"": ""Subscription""
        },
        {
          ""name"": ""Union""
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
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
          ""name"": ""deprecated"",
          ""description"": null,
          ""locations"": [
            ""FIELD_DEFINITION"",
            ""ENUM_VALUE""
          ],
          ""args"": [
            {
              ""name"": ""reason""
            }
          ]
        },
        {
          ""name"": ""include"",
          ""description"": null,
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ],
          ""args"": [
            {
              ""name"": ""if""
            }
          ]
        },
        {
          ""name"": ""skip"",
          ""description"": null,
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ],
          ""args"": [
            {
              ""name"": ""if""
            }
          ]
        },
        {
          ""name"": ""specifiedBy"",
          ""description"": null,
          ""locations"": [
            ""SCALAR""
          ],
          ""args"": [
            {
              ""name"": ""url""
            }
          ]
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
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
      ""kind"": ""ENUM"",
      ""name"": ""Enum"",
      ""description"": ""Description"",
      ""enumValues"": [
        {
          ""name"": ""VALUE1"",
          ""description"": ""Description"",
          ""isDeprecated"": false,
          ""deprecationReason"": null
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
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
      ""kind"": ""ENUM"",
      ""name"": ""Enum"",
      ""description"": ""Description"",
      ""enumValues"": [
        {
          ""name"": ""VALUE1"",
          ""description"": ""Description"",
          ""isDeprecated"": false,
          ""deprecationReason"": null
        },
        {
          ""name"": ""VALUE2"",
          ""description"": ""Description"",
          ""isDeprecated"": true,
          ""deprecationReason"": ""reason""
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
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
      ""kind"": ""INPUT_OBJECT"",
      ""name"": ""InputObject"",
      ""description"": ""Description""
    }
  },
  ""extensions"": null,
  ""errors"": null
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
      ""kind"": ""OBJECT"",
      ""name"": ""Object"",
      ""description"": ""Description"",
      ""fields"": [
        {
          ""name"": ""int""
        },
        {
          ""name"": ""nonNullInt""
        }
      ],
      ""interfaces"": [
        {
          ""name"": ""Interface""
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
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
          ""name"": ""int"",
          ""description"": null,
          ""isDeprecated"": false,
          ""deprecationReason"": null,
          ""type"": {
            ""name"": ""Int"",
            ""kind"": ""SCALAR"",
            ""ofType"": null
          },
          ""args"": []
        },
        {
          ""name"": ""nonNullInt"",
          ""description"": ""Description"",
          ""isDeprecated"": false,
          ""deprecationReason"": null,
          ""type"": {
            ""name"": null,
            ""kind"": ""NON_NULL"",
            ""ofType"": {
              ""name"": ""Int""
            }
          },
          ""args"": [
            {
              ""name"": ""arg1""
            }
          ]
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
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
          ""args"": []
        },
        {
          ""args"": [
            {
              ""name"": ""arg1"",
              ""description"": null,
              ""type"": {
                ""name"": ""Float""
              },
              ""defaultValue"": ""1""
            }
          ]
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
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
      ""name"": ""Union"",
      ""description"": ""Description"",
      ""possibleTypes"": [
        {
          ""name"": ""Object""
        },
        {
          ""name"": ""Object2""
        }
      ]
    }
  },
  ""extensions"": null,
  ""errors"": null
}");
        }
    }
}
using System.Threading.Tasks;
using tanka.graphql.introspection;
using tanka.graphql.tests.data;
using tanka.graphql.tests.data.starwars;
using Xunit;
using static tanka.graphql.Executor;
using static tanka.graphql.Parser;

namespace tanka.graphql.tests
{
    public class StarwarsFacts : IClassFixture<StarwarsFixture>
    {
        public StarwarsFacts(StarwarsFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly StarwarsFixture _fixture;

        [Fact]
        public async Task Introspect()
        {
            /* Given */
            var schema = _fixture.Schema;
            await schema.InitializeAsync().ConfigureAwait(false);

            /* When */
            var introspectionSchema = await graphql.introspection.Introspect.SchemaAsync(schema)
                .ConfigureAwait(false);

            var result = await ExecuteAsync(
                new ExecutionOptions()
                {
                    Document =  ParseDocument(graphql.introspection.Introspect.DefaultQuery),
                    Schema = introspectionSchema
                }).ConfigureAwait(false);

            /* Then */
            result.ShouldMatchJson(@"{
  ""data"": {
    ""__schema"": {
      ""directives"": [
        {
          ""name"": ""include"",
          ""description"": """",
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ],
          ""args"": [
            {
              ""name"": ""if"",
              ""description"": """",
              ""defaultValue"": null,
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Boolean"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""NON_NULL""
              }
            }
          ]
        },
        {
          ""name"": ""skip"",
          ""description"": """",
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ],
          ""args"": [
            {
              ""name"": ""if"",
              ""description"": """",
              ""defaultValue"": null,
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Boolean"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""NON_NULL""
              }
            }
          ]
        }
      ],
      ""mutationType"": {
        ""name"": ""Mutation""
      },
      ""queryType"": {
        ""name"": ""Query""
      },
      ""types"": [
        {
          ""enumValues"": null,
          ""name"": ""InputObject"",
          ""description"": ""Description"",
          ""possibleTypes"": null,
          ""inputFields"": [
            {
              ""name"": ""field1"",
              ""description"": ""Description"",
              ""defaultValue"": ""True"",
              ""type"": {
                ""name"": ""Boolean"",
                ""ofType"": null,
                ""kind"": ""SCALAR""
              }
            }
          ],
          ""interfaces"": null,
          ""fields"": null,
          ""kind"": ""INPUT_OBJECT""
        },
        {
          ""enumValues"": null,
          ""name"": ""Boolean"",
          ""description"": ""The `Boolean` scalar type represents `true` or `false`"",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""enumValues"": null,
          ""name"": ""Object2"",
          ""description"": ""Description"",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": [],
          ""fields"": [
            {
              ""name"": ""int"",
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Int"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""LIST""
              }
            }
          ],
          ""kind"": ""OBJECT""
        },
        {
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
              ""deprecationReason"": ""Deprecated""
            }
          ],
          ""name"": ""Enum"",
          ""description"": ""Description"",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""kind"": ""ENUM""
        },
        {
          ""enumValues"": null,
          ""name"": ""Union"",
          ""description"": ""Description"",
          ""possibleTypes"": [
            {
              ""name"": ""Object"",
              ""ofType"": null,
              ""kind"": ""OBJECT""
            },
            {
              ""name"": ""Object2"",
              ""ofType"": null,
              ""kind"": ""OBJECT""
            }
          ],
          ""inputFields"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""kind"": ""UNION""
        },
        {
          ""enumValues"": null,
          ""name"": ""Float"",
          ""description"": ""The `Float` scalar type represents signed double-precision fractional values as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)"",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""enumValues"": null,
          ""name"": ""Int"",
          ""description"": ""The `Int` scalar type represents non-fractional signed whole numeric values"",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""enumValues"": null,
          ""name"": ""Interface"",
          ""description"": ""Description"",
          ""possibleTypes"": [
            {
              ""name"": ""Object"",
              ""ofType"": null,
              ""kind"": ""OBJECT""
            }
          ],
          ""inputFields"": null,
          ""interfaces"": null,
          ""fields"": [
            {
              ""name"": ""int"",
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": ""Int"",
                ""ofType"": null,
                ""kind"": ""SCALAR""
              }
            }
          ],
          ""kind"": ""INTERFACE""
        },
        {
          ""enumValues"": null,
          ""name"": ""Object"",
          ""description"": ""Description"",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": [
            {
              ""name"": ""Interface"",
              ""ofType"": null,
              ""kind"": ""INTERFACE""
            }
          ],
          ""fields"": [
            {
              ""name"": ""int"",
              ""description"": ""Description"",
              ""args"": [
                {
                  ""name"": ""arg1"",
                  ""description"": ""Description"",
                  ""defaultValue"": ""1"",
                  ""type"": {
                    ""name"": ""Float"",
                    ""ofType"": null,
                    ""kind"": ""SCALAR""
                  }
                }
              ],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Int"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""NON_NULL""
              }
            }
          ],
          ""kind"": ""OBJECT""
        },
        {
          ""enumValues"": null,
          ""name"": ""Query"",
          ""description"": """",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": [],
          ""fields"": [
            {
              ""name"": ""object"",
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": ""Object"",
                ""ofType"": null,
                ""kind"": ""OBJECT""
              }
            },
            {
              ""name"": ""union"",
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": ""Union"",
                ""ofType"": null,
                ""kind"": ""UNION""
              }
            },
            {
              ""name"": ""enum"",
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": ""Enum"",
                ""ofType"": null,
                ""kind"": ""ENUM""
              }
            },
            {
              ""name"": ""listOfObject"",
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Object2"",
                  ""ofType"": null,
                  ""kind"": ""OBJECT""
                },
                ""kind"": ""LIST""
              }
            },
            {
              ""name"": ""nonNullObject"",
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Object"",
                  ""ofType"": null,
                  ""kind"": ""OBJECT""
                },
                ""kind"": ""NON_NULL""
              }
            },
            {
              ""name"": ""inputObjectArg"",
              ""description"": ""With inputObject arg"",
              ""args"": [
                {
                  ""name"": ""arg1"",
                  ""description"": """",
                  ""defaultValue"": null,
                  ""type"": {
                    ""name"": ""InputObject"",
                    ""ofType"": null,
                    ""kind"": ""INPUT_OBJECT""
                  }
                }
              ],
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Boolean"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""NON_NULL""
              }
            }
          ],
          ""kind"": ""OBJECT""
        },
        {
          ""enumValues"": null,
          ""name"": ""Mutation"",
          ""description"": """",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": [],
          ""fields"": [],
          ""kind"": ""OBJECT""
        },
        {
          ""enumValues"": null,
          ""name"": ""Subscription"",
          ""description"": """",
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""interfaces"": [],
          ""fields"": [],
          ""kind"": ""OBJECT""
        }
      ],
      ""subscriptionType"": {
        ""name"": ""Subscription""
      }
    }
  }
}");
        }

        [Fact]
        public async Task Mutate_add_human_leia()
        {
            /* Given */
            var starwars = new Starwars();

            var query = $@"
    mutation {{
        addHuman(human: {{name:""Leia""}}) {{
            id
            name
            homePlanet
            friends {{
                name
            }}
        }}
    }}
";


            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document =  ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
              ""data"": {
                ""addHuman"": {
                  ""id"": ""humans/leia"",
                  ""name"": ""Leia"",
                  ""homePlanet"": null,
                  ""friends"": []
                }
              }
            }");
        }

        [Fact]
        public async Task Query_character_luke()
        {
            /* Given */
            var starwars = new Starwars();

            var id = "\"humans/luke\"";
            var query = $@"{{
    character(id: {id}) {{
        id
        name
        appearsIn
    }}
}}";

            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document =  ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
                   ""data"": {
                    ""character"": {
                      ""id"": ""humans/luke"",
                      ""name"": ""Luke"",
                      ""appearsIn"": [
                        ""JEDI"",
                        ""EMPIRE"",
                        ""NEWHOPE""
                        ]
                     }
                    }
                 }");
        }

        [Fact]
        public async Task Query_typename_of_character_luke()
        {
            /* Given */
            var starwars = new Starwars();

            var id = "\"humans/luke\"";
            var query = $@"{{
    character(id: {id}) {{
        __typename
        id
        name
        appearsIn
    }}
}}";

            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document =  ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
                   ""data"": {
                    ""character"": {
                      ""__typename"":""Human"",
                      ""id"": ""humans/luke"",
                      ""name"": ""Luke"",
                      ""appearsIn"": [
                        ""JEDI"",
                        ""EMPIRE"",
                        ""NEWHOPE""
                        ]
                     }
                    }
                 }");
        }

        [Fact]
        public async Task Query_character_luke_skip_appearsIn()
        {
            /* Given */
            var starwars = new Starwars();

            var id = "\"humans/luke\"";
            var query = $@"{{
    character(id: {id}) {{
        id
        name
        appearsIn @skip(if: true)
    }}
}}";

            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document =  ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
                   ""data"": {
                    ""character"": {
                      ""id"": ""humans/luke"",
                      ""name"": ""Luke""
                     }
                    }
                 }");
        }

        [Fact]
        public async Task Query_character_luke_do_not_include_appearsIn()
        {
            /* Given */
            var starwars = new Starwars();

            var id = "\"humans/luke\"";
            var query = $@"{{
    character(id: {id}) {{
        id
        name
        appearsIn @include(if: false)
    }}
}}";

            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document =  ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
                   ""data"": {
                    ""character"": {
                      ""id"": ""humans/luke"",
                      ""name"": ""Luke""
                     }
                    }
                 }");
        }

        [Fact(Skip = "Leia pops in so this test is bit fragile")]
        public async Task Query_characters_with_friends()
        {
            /* Given */
            var starwars = new Starwars();

            var query = $@"{{
    characters {{
        id
        name
        friends {{
            id
            name
        }}
    }}
}}";
            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document =  ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""characters"": [
                      {
                        ""id"": ""humans/han"",
                        ""name"": ""Han"",
                        ""friends"": [
                          {
                            ""id"": ""humans/luke"",
                            ""name"": ""Luke""
                          }
                        ]
                      },
                      {
                        ""id"": ""humans/luke"",
                        ""name"": ""Luke"",
                        ""friends"": [
                          {
                            ""id"": ""humans/han"",
                            ""name"": ""Han""
                          }
                        ]
                      }
                    ]
                  }}");
        }

        [Fact]
        public async Task Query_human_luke()
        {
            /* Given */
            var starwars = new Starwars();

            var id = "\"humans/luke\"";
            var query = $@"

query humans
{{
    human(id: {id}) {{
        id
        name
        homePlanet
        ...friendsAndFriends
    }}
}}

fragment friendsAndFriends on Human {{
    friends {{
        name
        friends {{
            name
        }}
    }}
}}
";


            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document =  ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
              ""data"": {
                ""human"": {
                  ""id"": ""humans/luke"",
                  ""name"": ""Luke"",
                  ""homePlanet"": ""Tatooine"",
                  ""friends"": [
                        {
                            ""name"": ""Han"",
                            ""friends"": [
                                        {
                                          ""name"": ""Luke""
                                        }
                                      ]
                        }
                      ]
                }
              }
            }");
        }
    }
}
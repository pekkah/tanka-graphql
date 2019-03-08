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
            var starwars = new Starwars();
            var schema = _fixture.CreateSchema(starwars);

            /* When */
            var result = await ExecuteAsync(
                new ExecutionOptions()
                {
                    Document =  ParseDocument(graphql.introspection.Introspect.DefaultQuery),
                    Schema = schema
                }).ConfigureAwait(false);

            /* Then */
            result.ShouldMatchJson(
                @"{
  ""data"": {
    ""__schema"": {
      ""subscriptionType"": null,
      ""types"": [
        {
          ""enumValues"": null,
          ""kind"": ""SCALAR"",
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""String"",
          ""interfaces"": null,
          ""description"": ""The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text."",
          ""fields"": null
        },
        {
          ""kind"": ""SCALAR"",
          ""enumValues"": null,
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""Int"",
          ""description"": ""The `Int` scalar type represents non-fractional signed whole numeric values"",
          ""interfaces"": null,
          ""fields"": null
        },
        {
          ""enumValues"": null,
          ""kind"": ""SCALAR"",
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""Float"",
          ""description"": ""The `Float` scalar type represents signed double-precision fractional values as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)"",
          ""interfaces"": null,
          ""fields"": null
        },
        {
          ""enumValues"": null,
          ""kind"": ""SCALAR"",
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""Boolean"",
          ""interfaces"": null,
          ""description"": ""The `Boolean` scalar type represents `true` or `false`"",
          ""fields"": null
        },
        {
          ""kind"": ""SCALAR"",
          ""enumValues"": null,
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""ID"",
          ""description"": ""The ID scalar type represents a unique identifier, often used to refetch an object or as the key for a cache. The ID type is serialized in the same way as a String; however, it is not intended to be human‐readable. While it is often numeric, it should always serialize as a String."",
          ""interfaces"": null,
          ""fields"": null
        },
        {
          ""enumValues"": [
            {
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""name"": ""NEWHOPE"",
              ""description"": """"
            },
            {
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""name"": ""EMPIRE"",
              ""description"": """"
            },
            {
              ""isDeprecated"": false,
              ""deprecationReason"": null,
              ""name"": ""JEDI"",
              ""description"": """"
            }
          ],
          ""kind"": ""ENUM"",
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""Episode"",
          ""interfaces"": null,
          ""description"": """",
          ""fields"": null
        },
        {
          ""enumValues"": null,
          ""kind"": ""INTERFACE"",
          ""inputFields"": null,
          ""possibleTypes"": [
            {
              ""kind"": ""OBJECT"",
              ""ofType"": null,
              ""name"": ""Human""
            }
          ],
          ""name"": ""Character"",
          ""interfaces"": null,
          ""description"": ""Character in the movie"",
          ""fields"": [
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""id"",
              ""type"": {
                ""kind"": ""NON_NULL"",
                ""ofType"": {
                  ""kind"": ""SCALAR"",
                  ""ofType"": null,
                  ""name"": ""String""
                },
                ""name"": null
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""name"",
              ""type"": {
                ""kind"": ""NON_NULL"",
                ""ofType"": {
                  ""kind"": ""SCALAR"",
                  ""ofType"": null,
                  ""name"": ""String""
                },
                ""name"": null
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""friends"",
              ""type"": {
                ""kind"": ""LIST"",
                ""ofType"": {
                  ""kind"": ""INTERFACE"",
                  ""ofType"": null,
                  ""name"": ""Character""
                },
                ""name"": null
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""appearsIn"",
              ""type"": {
                ""kind"": ""LIST"",
                ""ofType"": {
                  ""kind"": ""ENUM"",
                  ""ofType"": null,
                  ""name"": ""Episode""
                },
                ""name"": null
              },
              ""description"": null
            }
          ]
        },
        {
          ""enumValues"": null,
          ""kind"": ""OBJECT"",
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""Human"",
          ""interfaces"": [
            {
              ""kind"": ""INTERFACE"",
              ""ofType"": null,
              ""name"": ""Character""
            }
          ],
          ""description"": ""Human character"",
          ""fields"": [
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""id"",
              ""type"": {
                ""kind"": ""NON_NULL"",
                ""ofType"": {
                  ""kind"": ""SCALAR"",
                  ""ofType"": null,
                  ""name"": ""String""
                },
                ""name"": null
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""name"",
              ""type"": {
                ""kind"": ""NON_NULL"",
                ""ofType"": {
                  ""kind"": ""SCALAR"",
                  ""ofType"": null,
                  ""name"": ""String""
                },
                ""name"": null
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""friends"",
              ""type"": {
                ""kind"": ""LIST"",
                ""ofType"": {
                  ""kind"": ""INTERFACE"",
                  ""ofType"": null,
                  ""name"": ""Character""
                },
                ""name"": null
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""appearsIn"",
              ""type"": {
                ""kind"": ""LIST"",
                ""ofType"": {
                  ""kind"": ""ENUM"",
                  ""ofType"": null,
                  ""name"": ""Episode""
                },
                ""name"": null
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""homePlanet"",
              ""type"": {
                ""kind"": ""SCALAR"",
                ""ofType"": null,
                ""name"": ""String""
              },
              ""description"": null
            }
          ]
        },
        {
          ""enumValues"": null,
          ""kind"": ""OBJECT"",
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""Query"",
          ""interfaces"": [],
          ""description"": null,
          ""fields"": [
            {
              ""isDeprecated"": false,
              ""args"": [
                {
                  ""defaultValue"": null,
                  ""name"": ""id"",
                  ""type"": {
                    ""kind"": ""NON_NULL"",
                    ""ofType"": {
                      ""kind"": ""SCALAR"",
                      ""ofType"": null,
                      ""name"": ""String""
                    },
                    ""name"": null
                  },
                  ""description"": null
                }
              ],
              ""deprecationReason"": null,
              ""name"": ""human"",
              ""type"": {
                ""kind"": ""OBJECT"",
                ""ofType"": null,
                ""name"": ""Human""
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [
                {
                  ""defaultValue"": null,
                  ""name"": ""id"",
                  ""type"": {
                    ""kind"": ""NON_NULL"",
                    ""ofType"": {
                      ""kind"": ""SCALAR"",
                      ""ofType"": null,
                      ""name"": ""String""
                    },
                    ""name"": null
                  },
                  ""description"": null
                }
              ],
              ""deprecationReason"": null,
              ""name"": ""character"",
              ""type"": {
                ""kind"": ""INTERFACE"",
                ""ofType"": null,
                ""name"": ""Character""
              },
              ""description"": null
            },
            {
              ""isDeprecated"": false,
              ""args"": [],
              ""deprecationReason"": null,
              ""name"": ""characters"",
              ""type"": {
                ""kind"": ""LIST"",
                ""ofType"": {
                  ""kind"": ""INTERFACE"",
                  ""ofType"": null,
                  ""name"": ""Character""
                },
                ""name"": null
              },
              ""description"": null
            }
          ]
        },
        {
          ""enumValues"": null,
          ""kind"": ""INPUT_OBJECT"",
          ""inputFields"": [
            {
              ""defaultValue"": null,
              ""name"": ""name"",
              ""type"": {
                ""kind"": ""NON_NULL"",
                ""ofType"": {
                  ""kind"": ""SCALAR"",
                  ""ofType"": null,
                  ""name"": ""String""
                },
                ""name"": null
              },
              ""description"": null
            }
          ],
          ""possibleTypes"": null,
          ""name"": ""HumanInput"",
          ""interfaces"": null,
          ""description"": null,
          ""fields"": null
        },
        {
          ""enumValues"": null,
          ""kind"": ""OBJECT"",
          ""inputFields"": null,
          ""possibleTypes"": null,
          ""name"": ""Mutation"",
          ""interfaces"": [],
          ""description"": null,
          ""fields"": [
            {
              ""isDeprecated"": false,
              ""args"": [
                {
                  ""defaultValue"": null,
                  ""name"": ""human"",
                  ""type"": {
                    ""kind"": ""INPUT_OBJECT"",
                    ""ofType"": null,
                    ""name"": ""HumanInput""
                  },
                  ""description"": null
                }
              ],
              ""deprecationReason"": null,
              ""name"": ""addHuman"",
              ""type"": {
                ""kind"": ""OBJECT"",
                ""ofType"": null,
                ""name"": ""Human""
              },
              ""description"": null
            }
          ]
        }
      ],
      ""queryType"": {
        ""name"": ""Query""
      },
      ""mutationType"": {
        ""name"": ""Mutation""
      },
      ""directives"": [
        {
          ""args"": [
            {
              ""defaultValue"": null,
              ""name"": ""if"",
              ""type"": {
                ""kind"": ""NON_NULL"",
                ""ofType"": {
                  ""kind"": ""SCALAR"",
                  ""ofType"": null,
                  ""name"": ""Boolean""
                },
                ""name"": null
              },
              ""description"": """"
            }
          ],
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ],
          ""name"": ""include"",
          ""description"": """"
        },
        {
          ""args"": [
            {
              ""defaultValue"": null,
              ""name"": ""if"",
              ""type"": {
                ""kind"": ""NON_NULL"",
                ""ofType"": {
                  ""kind"": ""SCALAR"",
                  ""ofType"": null,
                  ""name"": ""Boolean""
                },
                ""name"": null
              },
              ""description"": """"
            }
          ],
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ],
          ""name"": ""skip"",
          ""description"": """"
        }
      ]
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


            var executableSchema = _fixture.CreateSchema(starwars);
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

            var executableSchema = _fixture.CreateSchema(starwars);
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

            var executableSchema = _fixture.CreateSchema(starwars);
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
        public async Task Query_typename_of_characters()
        {
            /* Given */
            var starwars = new Starwars();

            var query = $@"{{
                    characters {{
                        __typename
                        id
                        name
                        appearsIn
                    }}
                }}";

            var executableSchema = _fixture.CreateSchema(starwars);
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
                        ""appearsIn"": [
                          ""JEDI"",
                          ""EMPIRE"",
                          ""NEWHOPE""
                        ],
                        ""name"": ""Han"",
                        ""id"": ""humans/han"",
                        ""__typename"": ""Human""
                      },
                      {
                        ""appearsIn"": [
                          ""JEDI"",
                          ""EMPIRE"",
                          ""NEWHOPE""
                        ],
                        ""name"": ""Luke"",
                        ""id"": ""humans/luke"",
                        ""__typename"": ""Human""
                      }
                    ]
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

            var executableSchema = _fixture.CreateSchema(starwars);
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

            var executableSchema = _fixture.CreateSchema(starwars);
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
            var executableSchema = _fixture.CreateSchema(starwars);
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


            var executableSchema = _fixture.CreateSchema(starwars);
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
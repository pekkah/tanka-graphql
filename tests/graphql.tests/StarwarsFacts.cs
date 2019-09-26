using System.Threading.Tasks;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.Tests.Data.Starwars;
using Xunit;
using static Tanka.GraphQL.Executor;
using static Tanka.GraphQL.Parser;

namespace Tanka.GraphQL.Tests
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
                    Document =  ParseDocument(GraphQL.Introspection.Introspect.DefaultQuery),
                    Schema = schema
                }).ConfigureAwait(false);

            /* Then */
            result.ShouldMatchJson(
                @"{
  ""data"": {
    ""__schema"": {
      ""queryType"": {
        ""name"": ""Query""
      },
      ""types"": [
        {
          ""description"": ""The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text."",
          ""kind"": ""SCALAR"",
          ""enumValues"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""String""
        },
        {
          ""description"": ""The `Int` scalar type represents non-fractional signed whole numeric values"",
          ""kind"": ""SCALAR"",
          ""enumValues"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Int""
        },
        {
          ""description"": ""The `Float` scalar type represents signed double-precision fractional values as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)"",
          ""kind"": ""SCALAR"",
          ""enumValues"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Float""
        },
        {
          ""description"": ""The `Boolean` scalar type represents `true` or `false`"",
          ""kind"": ""SCALAR"",
          ""enumValues"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Boolean""
        },
        {
          ""description"": ""The ID scalar type represents a unique identifier, often used to refetch an object or as the key for a cache. The ID type is serialized in the same way as a String; however, it is not intended to be human‐readable. While it is often numeric, it should always serialize as a String."",
          ""kind"": ""SCALAR"",
          ""enumValues"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""ID""
        },
        {
          ""description"": """",
          ""kind"": ""ENUM"",
          ""enumValues"": [
            {
              ""name"": ""NEWHOPE"",
              ""description"": """",
              ""deprecationReason"": null,
              ""isDeprecated"": false
            },
            {
              ""name"": ""EMPIRE"",
              ""description"": """",
              ""deprecationReason"": null,
              ""isDeprecated"": false
            },
            {
              ""name"": ""JEDI"",
              ""description"": """",
              ""deprecationReason"": null,
              ""isDeprecated"": false
            }
          ],
          ""interfaces"": null,
          ""fields"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Episode""
        },
        {
          ""description"": ""Character in the movie"",
          ""kind"": ""INTERFACE"",
          ""enumValues"": null,
          ""interfaces"": null,
          ""fields"": [
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""String"",
                  ""kind"": ""SCALAR""
                },
                ""name"": null,
                ""kind"": ""NON_NULL""
              },
              ""name"": ""id"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""String"",
                  ""kind"": ""SCALAR""
                },
                ""name"": null,
                ""kind"": ""NON_NULL""
              },
              ""name"": ""name"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""Character"",
                  ""kind"": ""INTERFACE""
                },
                ""name"": null,
                ""kind"": ""LIST""
              },
              ""name"": ""friends"",
              ""deprecationReason"": null,
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""Episode"",
                  ""kind"": ""ENUM""
                },
                ""name"": null,
                ""kind"": ""LIST""
              },
              ""name"": ""appearsIn"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [],
              ""isDeprecated"": false
            }
          ],
          ""possibleTypes"": [
            {
              ""ofType"": null,
              ""name"": ""Human"",
              ""kind"": ""OBJECT""
            }
          ],
          ""inputFields"": null,
          ""name"": ""Character""
        },
        {
          ""description"": ""Human character"",
          ""kind"": ""OBJECT"",
          ""enumValues"": null,
          ""interfaces"": [
            {
              ""ofType"": null,
              ""name"": ""Character"",
              ""kind"": ""INTERFACE""
            }
          ],
          ""fields"": [
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""String"",
                  ""kind"": ""SCALAR""
                },
                ""name"": null,
                ""kind"": ""NON_NULL""
              },
              ""name"": ""id"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""String"",
                  ""kind"": ""SCALAR""
                },
                ""name"": null,
                ""kind"": ""NON_NULL""
              },
              ""name"": ""name"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""Character"",
                  ""kind"": ""INTERFACE""
                },
                ""name"": null,
                ""kind"": ""LIST""
              },
              ""name"": ""friends"",
              ""deprecationReason"": null,
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""Episode"",
                  ""kind"": ""ENUM""
                },
                ""name"": null,
                ""kind"": ""LIST""
              },
              ""name"": ""appearsIn"",
              ""deprecationReason"": null,
              ""description"": """",
              ""args"": [],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""String"",
                ""kind"": ""SCALAR""
              },
              ""name"": ""homePlanet"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [],
              ""isDeprecated"": false
            }
          ],
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Human""
        },
        {
          ""description"": """",
          ""kind"": ""OBJECT"",
          ""enumValues"": null,
          ""interfaces"": [],
          ""fields"": [
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""Human"",
                ""kind"": ""OBJECT""
              },
              ""name"": ""human"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [
                {
                  ""defaultValue"": null,
                  ""type"": {
                    ""ofType"": {
                      ""ofType"": null,
                      ""name"": ""String"",
                      ""kind"": ""SCALAR""
                    },
                    ""name"": null,
                    ""kind"": ""NON_NULL""
                  },
                  ""name"": ""id"",
                  ""description"": """"
                }
              ],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""Character"",
                ""kind"": ""INTERFACE""
              },
              ""name"": ""character"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [
                {
                  ""defaultValue"": null,
                  ""type"": {
                    ""ofType"": {
                      ""ofType"": null,
                      ""name"": ""String"",
                      ""kind"": ""SCALAR""
                    },
                    ""name"": null,
                    ""kind"": ""NON_NULL""
                  },
                  ""name"": ""id"",
                  ""description"": """"
                }
              ],
              ""isDeprecated"": false
            },
            {
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""Character"",
                  ""kind"": ""INTERFACE""
                },
                ""name"": null,
                ""kind"": ""LIST""
              },
              ""name"": ""characters"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [],
              ""isDeprecated"": false
            }
          ],
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Query""
        },
        {
          ""description"": """",
          ""kind"": ""INPUT_OBJECT"",
          ""enumValues"": null,
          ""interfaces"": null,
          ""fields"": null,
          ""possibleTypes"": null,
          ""inputFields"": [
            {
              ""defaultValue"": null,
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""String"",
                  ""kind"": ""SCALAR""
                },
                ""name"": null,
                ""kind"": ""NON_NULL""
              },
              ""name"": ""name"",
              ""description"": """"
            }
          ],
          ""name"": ""HumanInput""
        },
        {
          ""description"": """",
          ""kind"": ""OBJECT"",
          ""enumValues"": null,
          ""interfaces"": [],
          ""fields"": [
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""Human"",
                ""kind"": ""OBJECT""
              },
              ""name"": ""addHuman"",
              ""description"": """",
              ""deprecationReason"": null,
              ""args"": [
                {
                  ""defaultValue"": null,
                  ""type"": {
                    ""ofType"": null,
                    ""name"": ""HumanInput"",
                    ""kind"": ""INPUT_OBJECT""
                  },
                  ""name"": ""human"",
                  ""description"": """"
                }
              ],
              ""isDeprecated"": false
            }
          ],
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Mutation""
        }
      ],
      ""subscriptionType"": null,
      ""directives"": [
        {
          ""name"": ""include"",
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ],
          ""description"": """",
          ""args"": [
            {
              ""defaultValue"": null,
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""Boolean"",
                  ""kind"": ""SCALAR""
                },
                ""name"": null,
                ""kind"": ""NON_NULL""
              },
              ""name"": ""if"",
              ""description"": """"
            }
          ]
        },
        {
          ""name"": ""skip"",
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ],
          ""description"": """",
          ""args"": [
            {
              ""defaultValue"": null,
              ""type"": {
                ""ofType"": {
                  ""ofType"": null,
                  ""name"": ""Boolean"",
                  ""kind"": ""SCALAR""
                },
                ""name"": null,
                ""kind"": ""NON_NULL""
              },
              ""name"": ""if"",
              ""description"": """"
            }
          ]
        }
      ],
      ""mutationType"": {
        ""name"": ""Mutation""
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

        [Fact]
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
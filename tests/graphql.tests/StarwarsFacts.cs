using System.Threading.Tasks;
using fugu.graphql.introspection;
using fugu.graphql.tests.data;
using fugu.graphql.tests.data.starwars;
using Xunit;
using static fugu.graphql.Executor;
using static fugu.graphql.Parser;

namespace fugu.graphql.tests
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
            var introspectionSchema = await Introspection.ExamineAsync(schema)
                .ConfigureAwait(false);

            var result = await ExecuteAsync(
                new ExecutionOptions()
                {
                    Document = ParseDocument(Introspection.Query),
                    Schema = introspectionSchema
                }).ConfigureAwait(false);

            /* Then */
            result.ShouldMatchJson(@"{
  ""errors"": null,
  ""data"": {
    ""__schema"": {
      ""subscriptionType"": null,
      ""types"": [
        {
          ""interfaces"": [],
          ""enumValues"": [
            {
              ""isDeprecated"": false,
              ""name"": ""NEWHOPE"",
              ""description"": """",
              ""deprecationReason"": null
            },
            {
              ""isDeprecated"": false,
              ""name"": ""EMPIRE"",
              ""description"": """",
              ""deprecationReason"": null
            },
            {
              ""isDeprecated"": false,
              ""name"": ""JEDI"",
              ""description"": """",
              ""deprecationReason"": null
            }
          ],
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Episode"",
          ""description"": null,
          ""fields"": null,
          ""kind"": ""ENUM""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""String"",
          ""description"": ""The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text."",
          ""fields"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
          ""possibleTypes"": [
            {
              ""ofType"": null,
              ""name"": ""Human"",
              ""kind"": ""OBJECT""
            }
          ],
          ""inputFields"": null,
          ""name"": ""Character"",
          ""description"": ""Character in the movie"",
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
              ""isDeprecated"": false,
              ""args"": [],
              ""name"": ""id"",
              ""description"": """",
              ""deprecationReason"": null
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
              ""isDeprecated"": false,
              ""args"": [],
              ""name"": ""name"",
              ""description"": """",
              ""deprecationReason"": null
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
              ""isDeprecated"": false,
              ""args"": [],
              ""name"": ""friends"",
              ""description"": """",
              ""deprecationReason"": null
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
              ""isDeprecated"": false,
              ""args"": [],
              ""name"": ""appearsIn"",
              ""description"": """",
              ""deprecationReason"": null
            }
          ],
          ""kind"": ""INTERFACE""
        },
        {
          ""interfaces"": [
            {
              ""ofType"": null,
              ""name"": ""Character"",
              ""kind"": ""INTERFACE""
            }
          ],
          ""enumValues"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Human"",
          ""description"": ""Human character"",
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
              ""isDeprecated"": false,
              ""args"": [],
              ""name"": ""id"",
              ""description"": """",
              ""deprecationReason"": null
            },
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""String"",
                ""kind"": ""SCALAR""
              },
              ""isDeprecated"": false,
              ""args"": [],
              ""name"": ""name"",
              ""description"": """",
              ""deprecationReason"": null
            },
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""String"",
                ""kind"": ""SCALAR""
              },
              ""args"": [],
              ""isDeprecated"": false,
              ""name"": ""homePlanet"",
              ""description"": """",
              ""deprecationReason"": null
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
              ""args"": [],
              ""isDeprecated"": false,
              ""name"": ""friends"",
              ""description"": """",
              ""deprecationReason"": null
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
              ""isDeprecated"": false,
              ""args"": [],
              ""name"": ""appearsIn"",
              ""description"": """",
              ""deprecationReason"": null
            }
          ],
          ""kind"": ""OBJECT""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Query"",
          ""description"": """",
          ""fields"": [
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""Human"",
                ""kind"": ""OBJECT""
              },
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
              ""isDeprecated"": false,
              ""name"": ""human"",
              ""description"": """",
              ""deprecationReason"": null
            },
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""Character"",
                ""kind"": ""INTERFACE""
              },
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
              ""isDeprecated"": false,
              ""name"": ""character"",
              ""description"": """",
              ""deprecationReason"": null
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
              ""isDeprecated"": false,
              ""args"": [],
              ""name"": ""characters"",
              ""description"": """",
              ""deprecationReason"": null
            }
          ],
          ""kind"": ""OBJECT""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Int"",
          ""description"": ""The `Int` scalar type represents non-fractional signed whole numeric values"",
          ""fields"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Float"",
          ""description"": ""The `Float` scalar type represents signed double-precision fractional values as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)"",
          ""fields"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Boolean"",
          ""description"": ""The `Boolean` scalar type represents `true` or `false`"",
          ""fields"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""ID"",
          ""description"": ""The ID scalar type represents a unique identifier, often used to refetch an object or as the key for a cache. The ID type is serialized in the same way as a String; however, it is not intended to be human‐readable. While it is often numeric, it should always serialize as a String."",
          ""fields"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
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
          ""name"": ""HumanInput"",
          ""description"": null,
          ""fields"": null,
          ""kind"": ""INPUT_OBJECT""
        },
        {
          ""interfaces"": [],
          ""enumValues"": null,
          ""possibleTypes"": null,
          ""inputFields"": null,
          ""name"": ""Mutation"",
          ""description"": """",
          ""fields"": [
            {
              ""type"": {
                ""ofType"": null,
                ""name"": ""Human"",
                ""kind"": ""OBJECT""
              },
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
              ""isDeprecated"": false,
              ""name"": ""addHuman"",
              ""description"": """",
              ""deprecationReason"": null
            }
          ],
          ""kind"": ""OBJECT""
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
          ],
          ""name"": ""skip"",
          ""description"": """",
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ]
        },
        {
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
          ],
          ""name"": ""include"",
          ""description"": """",
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ]
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


            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document = ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
              ""errors"": null,
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
                Document = ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{""errors"": null,
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
                Document = ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{""errors"": null,
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
                Document = ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{""errors"": null,
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
            var executableSchema = await _fixture.MakeExecutableAsync(starwars).ConfigureAwait(false);
            var options = new ExecutionOptions
            {
                Schema = executableSchema,
                Document = ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{""errors"": null,
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
                Document = ParseDocument(query),
                OperationName = null,
                InitialValue = null,
                VariableValues = null
            };

            /* When */
            var actual = await ExecuteAsync(options).ConfigureAwait(false);

            /* Then */
            actual.ShouldMatchJson(
                @"{
              ""errors"": null,
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
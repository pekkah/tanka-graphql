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
            result.ShouldMatchJson(@"
{
  ""data"": {
    ""__schema"": {
      ""mutationType"": {
        ""name"": ""Mutation""
      },
      ""types"": [
        {
          ""inputFields"": null,
          ""name"": ""Episode"",
          ""fields"": null,
          ""enumValues"": [
            {
              ""name"": ""NEWHOPE"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """"
            },
            {
              ""name"": ""EMPIRE"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """"
            },
            {
              ""name"": ""JEDI"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """"
            }
          ],
          ""interfaces"": [],
          ""description"": """",
          ""possibleTypes"": null,
          ""kind"": ""ENUM""
        },
        {
          ""name"": ""String"",
          ""inputFields"": null,
          ""fields"": null,
          ""enumValues"": null,
          ""interfaces"": [],
          ""description"": ""The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text."",
          ""possibleTypes"": null,
          ""kind"": ""SCALAR""
        },
        {
          ""inputFields"": null,
          ""name"": ""Character"",
          ""fields"": [
            {
              ""name"": ""id"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [],
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""String"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""NON_NULL""
              }
            },
            {
              ""name"": ""name"",
              ""deprecationReason"": null,
              ""description"": """",
              ""isDeprecated"": false,
              ""args"": [],
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""String"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""NON_NULL""
              }
            },
            {
              ""name"": ""friends"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [],
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Character"",
                  ""ofType"": null,
                  ""kind"": ""INTERFACE""
                },
                ""kind"": ""LIST""
              }
            },
            {
              ""name"": ""appearsIn"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [],
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Episode"",
                  ""ofType"": null,
                  ""kind"": ""ENUM""
                },
                ""kind"": ""LIST""
              }
            }
          ],
          ""enumValues"": null,
          ""interfaces"": [],
          ""description"": ""Character in the movie"",
          ""possibleTypes"": [
            {
              ""name"": ""Human"",
              ""ofType"": null,
              ""kind"": ""OBJECT""
            }
          ],
          ""kind"": ""INTERFACE""
        },
        {
          ""inputFields"": null,
          ""name"": ""Human"",
          ""fields"": [
            {
              ""name"": ""id"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [],
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""String"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""NON_NULL""
              }
            },
            {
              ""name"": ""name"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [],
              ""type"": {
                ""name"": ""String"",
                ""ofType"": null,
                ""kind"": ""SCALAR""
              }
            },
            {
              ""name"": ""homePlanet"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [],
              ""type"": {
                ""name"": ""String"",
                ""ofType"": null,
                ""kind"": ""SCALAR""
              }
            },
            {
              ""name"": ""friends"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [],
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Character"",
                  ""ofType"": null,
                  ""kind"": ""INTERFACE""
                },
                ""kind"": ""LIST""
              }
            },
            {
              ""name"": ""appearsIn"",
              ""deprecationReason"": null,
              ""description"": """",
              ""isDeprecated"": false,
              ""args"": [],
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Episode"",
                  ""ofType"": null,
                  ""kind"": ""ENUM""
                },
                ""kind"": ""LIST""
              }
            }
          ],
          ""enumValues"": null,
          ""interfaces"": [
            {
              ""name"": ""Character"",
              ""ofType"": null,
              ""kind"": ""INTERFACE""
            }
          ],
          ""description"": ""Human character"",
          ""possibleTypes"": null,
          ""kind"": ""OBJECT""
        },
        {
          ""inputFields"": null,
          ""name"": ""Query"",
          ""fields"": [
            {
              ""name"": ""human"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [
                {
                  ""name"": ""id"",
                  ""description"": """",
                  ""defaultValue"": null,
                  ""type"": {
                    ""name"": null,
                    ""ofType"": {
                      ""name"": ""String"",
                      ""ofType"": null,
                      ""kind"": ""SCALAR""
                    },
                    ""kind"": ""NON_NULL""
                  }
                }
              ],
              ""type"": {
                ""name"": ""Human"",
                ""ofType"": null,
                ""kind"": ""OBJECT""
              }
            },
            {
              ""name"": ""character"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [
                {
                  ""name"": ""id"",
                  ""description"": """",
                  ""defaultValue"": null,
                  ""type"": {
                    ""name"": null,
                    ""ofType"": {
                      ""name"": ""String"",
                      ""ofType"": null,
                      ""kind"": ""SCALAR""
                    },
                    ""kind"": ""NON_NULL""
                  }
                }
              ],
              ""type"": {
                ""name"": ""Character"",
                ""ofType"": null,
                ""kind"": ""INTERFACE""
              }
            },
            {
              ""name"": ""characters"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [],
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""Character"",
                  ""ofType"": null,
                  ""kind"": ""INTERFACE""
                },
                ""kind"": ""LIST""
              }
            }
          ],
          ""enumValues"": null,
          ""interfaces"": [],
          ""description"": """",
          ""possibleTypes"": null,
          ""kind"": ""OBJECT""
        },
        {
          ""inputFields"": [
            {
              ""name"": ""name"",
              ""description"": """",
              ""defaultValue"": null,
              ""type"": {
                ""name"": null,
                ""ofType"": {
                  ""name"": ""String"",
                  ""ofType"": null,
                  ""kind"": ""SCALAR""
                },
                ""kind"": ""NON_NULL""
              }
            }
          ],
          ""name"": ""HumanInput"",
          ""fields"": null,
          ""enumValues"": null,
          ""interfaces"": [],
          ""description"": null,
          ""possibleTypes"": null,
          ""kind"": ""INPUT_OBJECT""
        },
        {
          ""inputFields"": null,
          ""name"": ""Mutation"",
          ""fields"": [
            {
              ""name"": ""addHuman"",
              ""deprecationReason"": null,
              ""isDeprecated"": false,
              ""description"": """",
              ""args"": [
                {
                  ""name"": ""human"",
                  ""description"": """",
                  ""defaultValue"": null,
                  ""type"": {
                    ""name"": ""HumanInput"",
                    ""ofType"": null,
                    ""kind"": ""INPUT_OBJECT""
                  }
                }
              ],
              ""type"": {
                ""name"": ""Human"",
                ""ofType"": null,
                ""kind"": ""OBJECT""
              }
            }
          ],
          ""enumValues"": null,
          ""interfaces"": [],
          ""description"": """",
          ""possibleTypes"": null,
          ""kind"": ""OBJECT""
        }
      ],
      ""queryType"": {
        ""name"": ""Query""
      },
      ""directives"": [
        {
          ""name"": ""include"",
          ""description"": """",
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
          ],
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ]
        },
        {
          ""name"": ""skip"",
          ""description"": """",
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
          ],
          ""locations"": [
            ""FIELD"",
            ""FRAGMENT_SPREAD"",
            ""INLINE_FRAGMENT""
          ]
        }
      ],
      ""subscriptionType"": null
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
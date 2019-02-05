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
            var schema = await _fixture.MakeExecutableAsync(starwars);

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
                          ""queryType"": {
                            ""name"": ""Query""
                          },
                          ""directives"": [
                            {
                              ""args"": [
                                {
                                  ""defaultValue"": null,
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""Boolean"",
                                      ""kind"": ""SCALAR"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""name"": ""if""
                                }
                              ],
                              ""description"": """",
                              ""locations"": [
                                ""FIELD"",
                                ""FRAGMENT_SPREAD"",
                                ""INLINE_FRAGMENT""
                              ],
                              ""name"": ""include""
                            },
                            {
                              ""args"": [
                                {
                                  ""defaultValue"": null,
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""Boolean"",
                                      ""kind"": ""SCALAR"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""name"": ""if""
                                }
                              ],
                              ""description"": """",
                              ""locations"": [
                                ""FIELD"",
                                ""FRAGMENT_SPREAD"",
                                ""INLINE_FRAGMENT""
                              ],
                              ""name"": ""skip""
                            }
                          ],
                          ""types"": [
                            {
                              ""enumValues"": [
                                {
                                  ""deprecationReason"": null,
                                  ""description"": """",
                                  ""isDeprecated"": false,
                                  ""name"": ""NEWHOPE""
                                },
                                {
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""EMPIRE""
                                },
                                {
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""JEDI""
                                }
                              ],
                              ""inputFields"": null,
                              ""description"": """",
                              ""fields"": null,
                              ""name"": ""Episode"",
                              ""possibleTypes"": null,
                              ""kind"": ""ENUM"",
                              ""interfaces"": null
                            },
                            {
                              ""enumValues"": null,
                              ""inputFields"": null,
                              ""description"": ""The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text."",
                              ""fields"": null,
                              ""name"": ""String"",
                              ""kind"": ""SCALAR"",
                              ""possibleTypes"": null,
                              ""interfaces"": null
                            },
                            {
                              ""enumValues"": null,
                              ""inputFields"": null,
                              ""description"": ""Character in the movie"",
                              ""fields"": [
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""String"",
                                      ""kind"": ""SCALAR"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""id""
                                },
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""String"",
                                      ""kind"": ""SCALAR"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""name""
                                },
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""LIST"",
                                    ""ofType"": {
                                      ""name"": ""Character"",
                                      ""kind"": ""INTERFACE"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""friends""
                                },
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""LIST"",
                                    ""ofType"": {
                                      ""name"": ""Episode"",
                                      ""kind"": ""ENUM"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""appearsIn""
                                }
                              ],
                              ""name"": ""Character"",
                              ""possibleTypes"": [
                                {
                                  ""name"": ""Human"",
                                  ""kind"": ""OBJECT"",
                                  ""ofType"": null
                                }
                              ],
                              ""kind"": ""INTERFACE"",
                              ""interfaces"": null
                            },
                            {
                              ""enumValues"": null,
                              ""inputFields"": null,
                              ""description"": ""Human character"",
                              ""fields"": [
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""String"",
                                      ""kind"": ""SCALAR"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""id""
                                },
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": ""String"",
                                    ""kind"": ""SCALAR"",
                                    ""ofType"": null
                                  },
                                  ""deprecationReason"": null,
                                  ""description"": """",
                                  ""isDeprecated"": false,
                                  ""name"": ""name""
                                },
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": ""String"",
                                    ""kind"": ""SCALAR"",
                                    ""ofType"": null
                                  },
                                  ""deprecationReason"": null,
                                  ""description"": """",
                                  ""isDeprecated"": false,
                                  ""name"": ""homePlanet""
                                },
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""LIST"",
                                    ""ofType"": {
                                      ""name"": ""Character"",
                                      ""kind"": ""INTERFACE"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""friends""
                                },
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""LIST"",
                                    ""ofType"": {
                                      ""name"": ""Episode"",
                                      ""kind"": ""ENUM"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""deprecationReason"": null,
                                  ""description"": """",
                                  ""isDeprecated"": false,
                                  ""name"": ""appearsIn""
                                }
                              ],
                              ""name"": ""Human"",
                              ""possibleTypes"": null,
                              ""kind"": ""OBJECT"",
                              ""interfaces"": [
                                {
                                  ""name"": ""Character"",
                                  ""kind"": ""INTERFACE"",
                                  ""ofType"": null
                                }
                              ]
                            },
                            {
                              ""enumValues"": null,
                              ""inputFields"": null,
                              ""description"": """",
                              ""fields"": [
                                {
                                  ""args"": [
                                    {
                                      ""defaultValue"": null,
                                      ""type"": {
                                        ""name"": null,
                                        ""kind"": ""NON_NULL"",
                                        ""ofType"": {
                                          ""name"": ""String"",
                                          ""kind"": ""SCALAR"",
                                          ""ofType"": null
                                        }
                                      },
                                      ""description"": """",
                                      ""name"": ""id""
                                    }
                                  ],
                                  ""type"": {
                                    ""name"": ""Human"",
                                    ""kind"": ""OBJECT"",
                                    ""ofType"": null
                                  },
                                  ""deprecationReason"": null,
                                  ""description"": """",
                                  ""isDeprecated"": false,
                                  ""name"": ""human""
                                },
                                {
                                  ""args"": [
                                    {
                                      ""defaultValue"": null,
                                      ""type"": {
                                        ""name"": null,
                                        ""kind"": ""NON_NULL"",
                                        ""ofType"": {
                                          ""name"": ""String"",
                                          ""kind"": ""SCALAR"",
                                          ""ofType"": null
                                        }
                                      },
                                      ""description"": """",
                                      ""name"": ""id""
                                    }
                                  ],
                                  ""type"": {
                                    ""name"": ""Character"",
                                    ""kind"": ""INTERFACE"",
                                    ""ofType"": null
                                  },
                                  ""description"": """",
                                  ""deprecationReason"": null,
                                  ""isDeprecated"": false,
                                  ""name"": ""character""
                                },
                                {
                                  ""args"": [],
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""LIST"",
                                    ""ofType"": {
                                      ""name"": ""Character"",
                                      ""kind"": ""INTERFACE"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""deprecationReason"": null,
                                  ""description"": """",
                                  ""isDeprecated"": false,
                                  ""name"": ""characters""
                                }
                              ],
                              ""name"": ""Query"",
                              ""possibleTypes"": null,
                              ""kind"": ""OBJECT"",
                              ""interfaces"": []
                            },
                            {
                              ""enumValues"": null,
                              ""inputFields"": [
                                {
                                  ""defaultValue"": null,
                                  ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""String"",
                                      ""kind"": ""SCALAR"",
                                      ""ofType"": null
                                    }
                                  },
                                  ""description"": """",
                                  ""name"": ""name""
                                }
                              ],
                              ""description"": null,
                              ""fields"": null,
                              ""name"": ""HumanInput"",
                              ""possibleTypes"": null,
                              ""kind"": ""INPUT_OBJECT"",
                              ""interfaces"": null
                            },
                            {
                              ""enumValues"": null,
                              ""inputFields"": null,
                              ""description"": """",
                              ""fields"": [
                                {
                                  ""args"": [
                                    {
                                      ""defaultValue"": null,
                                      ""type"": {
                                        ""name"": ""HumanInput"",
                                        ""kind"": ""INPUT_OBJECT"",
                                        ""ofType"": null
                                      },
                                      ""description"": """",
                                      ""name"": ""human""
                                    }
                                  ],
                                  ""type"": {
                                    ""name"": ""Human"",
                                    ""kind"": ""OBJECT"",
                                    ""ofType"": null
                                  },
                                  ""deprecationReason"": null,
                                  ""description"": """",
                                  ""isDeprecated"": false,
                                  ""name"": ""addHuman""
                                }
                              ],
                              ""name"": ""Mutation"",
                              ""kind"": ""OBJECT"",
                              ""possibleTypes"": null,
                              ""interfaces"": []
                            }
                          ],
                          ""subscriptionType"": null,
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
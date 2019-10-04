using System.Linq;
using Xunit;

namespace Tanka.GraphQL.Server.Links.Tests.Introspection
{
    public class ParseIntrospectionFacts
    {
        public ParseIntrospectionFacts()
        {
            IntrospectionJson = @"{
          ""data"": {
            ""__schema"": {
              ""directives"": [
                {
                  ""args"": [
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""SCALAR"",
                          ""name"": ""Boolean""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""description"": """",
                      ""name"": ""if"",
                      ""defaultValue"": null
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
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""SCALAR"",
                          ""name"": ""Boolean""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""description"": """",
                      ""name"": ""if"",
                      ""defaultValue"": null
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
              ""queryType"": {
                ""name"": ""Query""
              },
              ""subscriptionType"": {
                ""name"": ""Subscription""
              },
              ""mutationType"": {
                ""name"": ""Mutation""
              },
              ""types"": [
                {
                  ""interfaces"": null,
                  ""inputFields"": null,
                  ""description"": ""The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text."",
                  ""kind"": ""SCALAR"",
                  ""enumValues"": null,
                  ""fields"": null,
                  ""possibleTypes"": null,
                  ""name"": ""String""
                },
                {
                  ""interfaces"": null,
                  ""inputFields"": null,
                  ""description"": ""The `Int` scalar type represents non-fractional signed whole numeric values"",
                  ""kind"": ""SCALAR"",
                  ""enumValues"": null,
                  ""fields"": null,
                  ""possibleTypes"": null,
                  ""name"": ""Int""
                },
                {
                  ""interfaces"": null,
                  ""inputFields"": null,
                  ""description"": ""The `Float` scalar type represents signed double-precision fractional values as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)"",
                  ""kind"": ""SCALAR"",
                  ""enumValues"": null,
                  ""fields"": null,
                  ""possibleTypes"": null,
                  ""name"": ""Float""
                },
                {
                  ""interfaces"": null,
                  ""inputFields"": null,
                  ""description"": ""The `Boolean` scalar type represents `true` or `false`"",
                  ""kind"": ""SCALAR"",
                  ""enumValues"": null,
                  ""fields"": null,
                  ""possibleTypes"": null,
                  ""name"": ""Boolean""
                },
                {
                  ""interfaces"": null,
                  ""inputFields"": null,
                  ""description"": ""The ID scalar type represents a unique identifier, often used to refetch an object or as the key for a cache. The ID type is serialized in the same way as a String; however, it is not intended to be human‐readable. While it is often numeric, it should always serialize as a String."",
                  ""kind"": ""SCALAR"",
                  ""enumValues"": null,
                  ""fields"": null,
                  ""possibleTypes"": null,
                  ""name"": ""ID""
                },
                {
                  ""interfaces"": null,
                  ""inputFields"": [
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""SCALAR"",
                          ""name"": ""String""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""description"": """",
                      ""name"": ""content"",
                      ""defaultValue"": null
                    }
                  ],
                  ""description"": """",
                  ""kind"": ""INPUT_OBJECT"",
                  ""enumValues"": null,
                  ""fields"": null,
                  ""possibleTypes"": null,
                  ""name"": ""InputMessage""
                },
                {
                  ""interfaces"": [],
                  ""inputFields"": null,
                  ""description"": """",
                  ""kind"": ""OBJECT"",
                  ""fields"": [
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""SCALAR"",
                          ""name"": ""ID""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""args"": [],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""userId"",
                      ""deprecationReason"": null
                    },
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""SCALAR"",
                          ""name"": ""String""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""args"": [],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""name"",
                      ""deprecationReason"": null
                    }
                  ],
                  ""enumValues"": null,
                  ""possibleTypes"": null,
                  ""name"": ""From""
                },
                {
                  ""interfaces"": [],
                  ""inputFields"": null,
                  ""description"": """",
                  ""kind"": ""OBJECT"",
                  ""fields"": [
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""SCALAR"",
                          ""name"": ""ID""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""args"": [],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""id"",
                      ""deprecationReason"": null
                    },
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""OBJECT"",
                          ""name"": ""From""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""args"": [],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""from"",
                      ""deprecationReason"": null
                    },
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""SCALAR"",
                          ""name"": ""String""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""args"": [],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""content"",
                      ""deprecationReason"": null
                    },
                    {
                      ""type"": {
                        ""ofType"": null,
                        ""kind"": ""SCALAR"",
                        ""name"": ""String""
                      },
                      ""args"": [],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""timestamp"",
                      ""deprecationReason"": null
                    }
                  ],
                  ""enumValues"": null,
                  ""possibleTypes"": null,
                  ""name"": ""Message""
                },
                {
                  ""interfaces"": [],
                  ""inputFields"": null,
                  ""description"": """",
                  ""kind"": ""OBJECT"",
                  ""fields"": [
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""OBJECT"",
                          ""name"": ""Message""
                        },
                        ""kind"": ""LIST"",
                        ""name"": null
                      },
                      ""args"": [],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""messages"",
                      ""deprecationReason"": null
                    }
                  ],
                  ""enumValues"": null,
                  ""possibleTypes"": null,
                  ""name"": ""Query""
                },
                {
                  ""interfaces"": [],
                  ""inputFields"": null,
                  ""description"": """",
                  ""kind"": ""OBJECT"",
                  ""fields"": [
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""OBJECT"",
                          ""name"": ""Message""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""args"": [
                        {
                          ""type"": {
                            ""ofType"": {
                              ""ofType"": null,
                              ""kind"": ""INPUT_OBJECT"",
                              ""name"": ""InputMessage""
                            },
                            ""kind"": ""NON_NULL"",
                            ""name"": null
                          },
                          ""description"": """",
                          ""name"": ""message"",
                          ""defaultValue"": null
                        }
                      ],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""addMessage"",
                      ""deprecationReason"": null
                    },
                    {
                      ""type"": {
                        ""ofType"": {
                          ""ofType"": null,
                          ""kind"": ""OBJECT"",
                          ""name"": ""Message""
                        },
                        ""kind"": ""NON_NULL"",
                        ""name"": null
                      },
                      ""args"": [
                        {
                          ""type"": {
                            ""ofType"": {
                              ""ofType"": null,
                              ""kind"": ""SCALAR"",
                              ""name"": ""ID""
                            },
                            ""kind"": ""NON_NULL"",
                            ""name"": null
                          },
                          ""description"": """",
                          ""name"": ""id"",
                          ""defaultValue"": null
                        },
                        {
                          ""type"": {
                            ""ofType"": {
                              ""ofType"": null,
                              ""kind"": ""INPUT_OBJECT"",
                              ""name"": ""InputMessage""
                            },
                            ""kind"": ""NON_NULL"",
                            ""name"": null
                          },
                          ""description"": """",
                          ""name"": ""message"",
                          ""defaultValue"": null
                        }
                      ],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""editMessage"",
                      ""deprecationReason"": null
                    }
                  ],
                  ""enumValues"": null,
                  ""possibleTypes"": null,
                  ""name"": ""Mutation""
                },
                {
                  ""interfaces"": [],
                  ""inputFields"": null,
                  ""description"": """",
                  ""kind"": ""OBJECT"",
                  ""fields"": [
                    {
                      ""type"": {
                        ""ofType"": null,
                        ""kind"": ""OBJECT"",
                        ""name"": ""Message""
                      },
                      ""args"": [],
                      ""description"": """",
                      ""isDeprecated"": false,
                      ""name"": ""messages"",
                      ""deprecationReason"": null
                    }
                  ],
                  ""enumValues"": null,
                  ""possibleTypes"": null,
                  ""name"": ""Subscription""
                }
              ]
            }
          },
          ""extensions"": {}
        }";
        }

        public string IntrospectionJson { get; }

        [Fact]
        public void Parse_Schema()
        {
            /* Given */
            /* When */
            var result = IntrospectionParser.Deserialize(IntrospectionJson);

            /* Then */
            Assert.NotNull(result.Schema);
        }

        [Fact]
        public void Parse_QueryType_Name()
        {
            /* Given */
            /* When */
            var result = IntrospectionParser.Deserialize(IntrospectionJson);

            /* Then */
            Assert.NotNull(result.Schema.QueryType.Name);
        }

        [Fact]
        public void Parse_MutationType_Name()
        {
            /* Given */
            /* When */
            var result = IntrospectionParser.Deserialize(IntrospectionJson);

            /* Then */
            Assert.NotNull(result.Schema.MutationType.Name);
        }

        [Fact]
        public void Parse_SubscriptionType_Name()
        {
            /* Given */
            /* When */
            var result = IntrospectionParser.Deserialize(IntrospectionJson);

            /* Then */
            Assert.NotNull(result.Schema.SubscriptionType.Name);
        }

        [Fact]
        public void Parsed_Types_Includes_QueryType()
        {
            /* Given */
            /* When */
            var result = IntrospectionParser.Deserialize(IntrospectionJson);

            /* Then */
            Assert.NotNull(result.Schema.Types.SingleOrDefault(t => t.Name == result.Schema.QueryType.Name));
        }

        [Fact]
        public void Parsed_Types_Includes_MutationType()
        {
            /* Given */
            /* When */
            var result = IntrospectionParser.Deserialize(IntrospectionJson);

            /* Then */
            Assert.NotNull(result.Schema.Types.SingleOrDefault(t => t.Name == result.Schema.MutationType.Name));
        }

        [Fact]
        public void Parsed_Types_Includes_SubscriptionType()
        {
            /* Given */
            /* When */
            var result = IntrospectionParser.Deserialize(IntrospectionJson);

            /* Then */
            Assert.NotNull(result.Schema.Types.SingleOrDefault(t => t.Name == result.Schema.SubscriptionType.Name));
        }

        [Fact]
        public void Parsed_Types_have_kind()
        {
            /* Given */
            /* When */
            var result = IntrospectionParser.Deserialize(IntrospectionJson);

            /* Then */
            Assert.All(result.Schema.Types, type => Assert.NotNull(type.Kind));
        }
    }
}
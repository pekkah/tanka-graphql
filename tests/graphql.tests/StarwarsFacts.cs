using System.Threading.Tasks;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.Tests.Data.Starwars;
using Xunit;
using static Tanka.GraphQL.Executor;

namespace Tanka.GraphQL.Tests;

public class StarwarsFacts : IClassFixture<StarwarsFixture>
{
    private readonly StarwarsFixture _fixture;

    public StarwarsFacts(StarwarsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Introspect()
    {
        /* Given */
        var starwars = new Starwars();
        var schema = await _fixture.CreateSchema(starwars);

        /* When */
        var result = await ExecuteAsync(
            new ExecutionOptions
            {
                Document = GraphQL.Introspection.Introspect.DefaultQuery,
                Schema = schema,
                IncludeExceptionDetails = true
            }).ConfigureAwait(false);

        /* Then */
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result?.Data);

        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Mutate_add_human_leia()
    {
        /* Given */
        var starwars = new Starwars();

        var query = @"
    mutation {
        addHuman(human: {name:""Leia""}) {
            id
            name
            homePlanet
            friends {
                name
            }
        }
    }
";


        var executableSchema = await _fixture.CreateSchema(starwars);
        var options = new ExecutionOptions
        {
            Schema = executableSchema,
            Document = query,
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

        var executableSchema = await _fixture.CreateSchema(starwars);
        var options = new ExecutionOptions
        {
            Schema = executableSchema,
            Document = query,
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

        var executableSchema = await _fixture.CreateSchema(starwars);
        var options = new ExecutionOptions
        {
            Schema = executableSchema,
            Document = query,
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

        var query = @"{
                    characters {
                        __typename
                        id
                        name
                        appearsIn
                    }
                }";

        var executableSchema = await _fixture.CreateSchema(starwars);
        var options = new ExecutionOptions
        {
            Schema = executableSchema,
            Document = query,
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

        var executableSchema = await _fixture.CreateSchema(starwars);
        var options = new ExecutionOptions
        {
            Schema = executableSchema,
            Document = query,
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

        var executableSchema = await _fixture.CreateSchema(starwars);
        var options = new ExecutionOptions
        {
            Schema = executableSchema,
            Document = query,
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

        var query = @"{
    characters {
        id
        name
        friends {
            id
            name
        }
    }
}";
        var executableSchema = await _fixture.CreateSchema(starwars);
        var options = new ExecutionOptions
        {
            Schema = executableSchema,
            Document = query,
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


        var executableSchema = await _fixture.CreateSchema(starwars);
        var options = new ExecutionOptions
        {
            Schema = executableSchema,
            Document = query,
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
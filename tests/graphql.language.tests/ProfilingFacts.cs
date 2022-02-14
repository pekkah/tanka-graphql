using System.Text;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

public class ProfilingFacts
{
    public static string DefaultQuery = @"
            query IntrospectionQuery {
              __schema {
                queryType { name }
                mutationType { name }
                subscriptionType { name }
                types {
                  ...FullType
                }
                directives {
                    name
                    description
                    locations
                    args {
                        ...InputValue
                    }
                }
              }
            }
            fragment FullType on __Type {
              kind
              name
              description
              fields(includeDeprecated: true) {
                name
                description
                args {
                  ...InputValue
                }
                type {
                  ...TypeRef
                }
                isDeprecated
                deprecationReason
              }
              inputFields {
                ...InputValue
              }
              interfaces {
                ...TypeRef
              }
              enumValues(includeDeprecated: true) {
                name
                description
                isDeprecated
                deprecationReason
              }
              possibleTypes {
                ...TypeRef
              }
            }
            fragment InputValue on __InputValue {
              name
              description
              type { ...TypeRef }
              defaultValue
            }
            fragment TypeRef on __Type {
              kind
              name
              ofType {
                kind
                name
                ofType {
                  kind
                  name
                  ofType {
                    kind
                    name
                    ofType {
                      kind
                      name
                      ofType {
                        kind
                        name
                        ofType {
                          kind
                          name
                          ofType {
                            kind
                            name
                          }
                        }
                      }
                    }
                  }
                }
              }
            }";

    private readonly byte[] _queryBytes;

    public ProfilingFacts()
    {
        _queryBytes = Encoding.UTF8.GetBytes(DefaultQuery);
    }

    [Fact]
    public void Lex_IntrospectionQuery()
    {
        var lexer = Lexer.Create(_queryBytes);

        while (lexer.Advance())
            if (lexer.Kind == TokenKind.LeftBrace)
            {
            }
    }

    [Fact]
    public void Parse_IntrospectionQuery()
    {
        var parser = Parser.Create(_queryBytes);
        var document = parser.ParseExecutableDocument();

        Assert.NotNull(document.OperationDefinitions);
        Assert.NotEmpty(document.OperationDefinitions);
    }
}
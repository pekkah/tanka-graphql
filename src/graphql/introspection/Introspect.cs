using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Introspection
{
    public class Introspect
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

        /// <summary>
        ///     Return introspection schema for given schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static ISchema Schema(ISchema schema)
        {
            var introspectionSchema = IntrospectionSchema.Create();
            var introspectionResolvers = new IntrospectionResolvers(schema);

            return SchemaTools.MakeExecutableSchema(
                introspectionSchema,
                introspectionResolvers);
        }
    }
}
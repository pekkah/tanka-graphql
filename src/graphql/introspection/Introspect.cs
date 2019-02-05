using System.Threading.Tasks;
using tanka.graphql.tools;
using tanka.graphql.type;

namespace tanka.graphql.introspection
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

        public static async Task<ISchema> SchemaAsync(ISchema schema)
        {
            if (!schema.IsInitialized)
                await schema.InitializeAsync();

            var introspectionSchema = IntrospectionSchema.Build();
            await introspectionSchema.InitializeAsync();
            
            var introspectionResolvers = new IntrospectionResolvers(schema);
            return await SchemaTools.MakeExecutableSchemaAsync(
                introspectionSchema,
                introspectionResolvers);
        }
    }
}
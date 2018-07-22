using System.Threading.Tasks;
using fugu.graphql.type;

namespace fugu.graphql.introspection
{
    public static class Introspection
    {
        public static string Query = @"query IntrospectionQuery {
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

        public static async Task<ExecutableSchema> ExamineAsync(ISchema schema)
        {
            if (!schema.IsInitialized)
                await schema.InitializeAsync();

            var data = Examiner.Examine(schema);
            var introspectionSchema = new IntrospectionSchema();
            var resolvers = new IntrospectionResolvers(
                data,
                schema);

            return await ExecutableSchema.MakeExecutableSchemaAsync(
                introspectionSchema,
                resolvers,
                resolvers);
        }
    }
}
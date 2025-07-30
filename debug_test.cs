using System;
using System.Threading.Tasks;
using Tanka.GraphQL;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

class Program
{
    static async Task Main()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            input UserInput {
                name: String!
                age: Int!
                email: String = ""noemail@example.com""
            }

            type Query {
                processUser(input: UserInput!): String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "processUser", context => {
                    var input = context.ArgumentValue<object>("input");
                    return context.ResolveAs("test");
                }}
            }
        };

        var schema = await builder.Build(resolvers);

        // Test query missing required field 'name'
        var query = @"{ 
            processUser(input: { age: 30, email: ""john@example.com"" }) 
        }";

        var result = await Executor.Execute(schema, query);
        
        Console.WriteLine($"Errors count: {result.Errors?.Count ?? 0}");
        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error: {error.Message}");
            }
        }
    }
}
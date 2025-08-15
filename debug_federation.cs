using System;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Extensions.ApolloFederation;

public class Program
{
    public static async Task Main(string[] args)
    {
        var schema = @"
type Product @key(fields: ""id"") {
    id: ID!
    name: String
}

type Query {
    product(id: ID!): Product
}";

        var subgraphOptions = new SubgraphOptions(new DictionaryReferenceResolversMap());
        var builder = new ExecutableSchemaBuilder()
            .Add(schema)
            .AddSubgraph(subgraphOptions);

        var result = await builder.BuildWithFederation();

        Console.WriteLine("Schema built successfully!");
        Console.WriteLine($"Schema has {result.Types.Count()} types:");
        
        foreach (var type in result.Types.OrderBy(t => t.Name))
        {
            Console.WriteLine($"  - {type.Name}");
        }
        
        // Check specifically for Federation types
        var fieldSetType = result.GetNamedType("FieldSet");
        var anyType = result.GetNamedType("_Any");
        var serviceType = result.GetNamedType("_Service");
        var entityType = result.GetNamedType("_Entity");
        
        Console.WriteLine($"FieldSet type: {(fieldSetType != null ? "Found" : "Not found")}");
        Console.WriteLine($"_Any type: {(anyType != null ? "Found" : "Not found")}");
        Console.WriteLine($"_Service type: {(serviceType != null ? "Found" : "Not found")}");
        Console.WriteLine($"_Entity type: {(entityType != null ? "Found" : "Not found")}");
        
        // Check Query type fields
        var queryType = result.Query;
        var queryFields = result.GetFields(queryType.Name);
        Console.WriteLine($"Query fields: {string.Join(", ", queryFields.Keys)}");
    }
}
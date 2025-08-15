using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Middleware that handles type collection and organization
/// </summary>
public class BuiltInTypesMiddleware : ISchemaBuildMiddleware
{
    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        var options = context.Options;

        // Add built-in types if requested and not already present
        if (options.IncludeBuiltInTypes)
        {
            var existingTypes = context.Builder.GetTypeDefinitions();
            var hasBuiltInTypes = existingTypes.Any(t =>
                t.Name.Value == "String" ||
                t.Name.Value == "Int" ||
                t.Name.Value == "Boolean" ||
                t.Name.Value == "Float" ||
                t.Name.Value == "ID");

            if (!hasBuiltInTypes)
            {
                context.Builder.Add(SchemaBuilder.BuiltInTypes);
            }
        }

        return await next(context);
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem.SchemaValidation;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Middleware that handles schema validation
/// </summary>
public class ValidationMiddleware : ISchemaBuildMiddleware
{
    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        var options = context.Options;

        // Run schema validation if rules are configured
        if (options.SchemaValidationRules?.Any() == true)
        {
            var typeDefinitions = context.Builder.GetTypeDefinitions(context.Options.BuildTypesFromOrphanedExtensions).ToList();

            var validator = new SchemaValidator(options.SchemaValidationRules);
            var validationResult = validator.Validate(typeDefinitions);

            if (!validationResult.IsValid)
            {
                throw new SchemaValidationException(validationResult.Errors);
            }
        }

        return await next(context);
    }
}
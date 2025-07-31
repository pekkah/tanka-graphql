using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public interface ISchemaValidator
{
    SchemaValidationResult Validate(IEnumerable<TypeDefinition> typeDefinitions);
}
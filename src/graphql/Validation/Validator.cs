using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation;

public interface IValidator3
{
    ValueTask<ValidationResult> Validate(
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variables);
}

public class Validator3 : IValidator3
{
    private readonly IEnumerable<CombineRule> _rules;

    public Validator3(IEnumerable<CombineRule> rules)
    {
        _rules = rules;
    }

    public ValueTask<ValidationResult> Validate(
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variables)
    {
        var visitor = new RulesWalker(
            _rules,
            schema,
            document,
            variables);

        return new ValueTask<ValidationResult>(visitor.Validate());
    }
}

public static class Validator
{
    public static ValidationResult Validate(
        IEnumerable<CombineRule> rules,
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variableValues = null)
    {
        var visitor = new RulesWalker(
            rules,
            schema,
            document,
            variableValues);

        return visitor.Validate();
    }
}
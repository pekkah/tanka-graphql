using Microsoft.Extensions.Options;

using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation;

public interface IAsyncValidator
{
    ValueTask<ValidationResult> Validate(
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variables);

    ValueTask<ValidationResult> Validate(
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variables,
        IServiceProvider? requestServices);
}

public class AsyncValidatorOptions
{
    public List<CombineRule> Rules { get; set; } = [.. ExecutionRules.All];
}

public class AsyncValidator : IAsyncValidator
{
    private readonly IOptions<AsyncValidatorOptions> _optionsMonitor;

    public ValueTask<ValidationResult> Validate(
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variables)
    {
        return Validate(schema, document, variables, null);
    }

    public ValueTask<ValidationResult> Validate(
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variables,
        IServiceProvider? requestServices)
    {
        var visitor = new RulesWalker(
            _optionsMonitor.Value.Rules,
            schema,
            document,
            variables,
            requestServices);

        return new(visitor.Validate());
    }

    public AsyncValidator(IEnumerable<CombineRule> rules) : this(Options.Create(new AsyncValidatorOptions() { Rules = [.. rules] }))
    {
    }

    public AsyncValidator(IOptions<AsyncValidatorOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }
}

[Obsolete("Use AsyncValidator")]
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
using System.Linq;
using System.Text.Json;

using Tanka.GraphQL.Server.SourceGenerators.Internal;

namespace Tanka.GraphQL.Server.SourceGenerators;

public record ObjectMethodDefinition
{
    public required string Name { get; set; }

    public MethodType Type { get; set; }

    public EquatableArray<ParameterDefinition> Parameters { get; set; } = new();

    public required string ReturnType { get; init; }

    public required string ClosestMatchingGraphQLTypeName { get; set; }

    public bool IsStatic { get; set; }

    /// <summary>
    ///     Should await the method call?
    /// </summary>
    public bool IsAsync => Type switch
    {
        MethodType.Void => false,
        MethodType.EnumerableT => false,
        MethodType.T => false,
        MethodType.Unknown => false,
        _ => true
    };

    public bool IsSubscription => Type is MethodType.AsyncEnumerableOfT;

    public string AsField => $"{JsonNamingPolicy.CamelCase.ConvertName(Name)}{AsFieldArguments}: {ClosestMatchingGraphQLTypeName}";

    public string AsFieldArguments => Parameters.Any(p => p.IsArgument)
        ? $"({string.Join(", ", Parameters.Where(p => p.IsArgument).Select(p => p.AsArgument))})"
        : string.Empty;

    public string ResolverName => IsSubscription ? $"Resolve{Name}" : Name;

    public string? SubscriberName => IsSubscription ? Name : null;
}

public enum MethodType
{
    /// <summary>
    ///     Task Method(...)
    /// </summary>
    Task,

    /// <summary>
    ///     ValueTask Method(...)
    /// </summary>
    ValueTask,

    /// <summary>
    ///     Task&lt;T&gt; Method(...)  
    /// </summary>
    TaskOfT,

    /// <summary>
    ///     ValueTask&lt;T&gt; Method(...)  
    /// </summary>
    ValueTaskOfT,

    /// <summary>
    ///     IAsyncEnumerable&lt;T&gt; Method(...)
    /// </summary>
    AsyncEnumerableOfT,

    /// <summary>
    ///     void Method(...)
    /// </summary>
    Void,

    /// <summary>
    ///     T Method(...)
    /// </summary>
    T,

    /// <summary>
    ///     IEnumerable&lt;T&gt; Method(...)
    /// </summary>
    EnumerableT,

    Unknown
}
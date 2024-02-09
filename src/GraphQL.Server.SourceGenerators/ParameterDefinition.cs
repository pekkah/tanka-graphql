namespace Tanka.GraphQL.Server.SourceGenerators;

public record ParameterDefinition
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public bool IsNullable { get; set; } = false;

    public bool? FromServices { get; set; }

    public bool? FromArguments { get; set; }

    public bool IsPrimitive { get; init; }
    
    public string? ClosestMatchingGraphQLTypeName { get; set; }

    public string AsArgument => $"{Name}: {ClosestMatchingGraphQLTypeName}";
    
    public bool IsArgument
    {
        get
        {
            if (FromArguments == true)
            {
                return true;
            }

            if (FromServices == true)
            {
                return false;
            }

            if (Type.EndsWith("ResolverContext"))
            {
                return false;
            }

            if (Type.EndsWith("SubscriberContext"))
            {
                return false;
            }

            if (Type.EndsWith("CancellationToken"))
            {
                return false;
            }

            if (Type.EndsWith("IServiceProvider"))
            {
                return false;
            }

            return IsPrimitive || true;
        }
    }
}
namespace Tanka.GraphQL;

public record ExecutorOptions
{
    public required ISchema Schema { get; set; }

    public bool TraceEnabled { get; set; } = false;

    public bool ValidationEnabled { get; set; } = true;

    public IServiceProvider? ServiceProvider { get; set; }
}
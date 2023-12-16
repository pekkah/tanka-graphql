namespace Tanka.GraphQL;

public record ExecutorOptions
{
    /// <summary>
    ///     Schema to use for execution
    /// </summary>
    public required ISchema Schema { get; set; }

    /// <summary>
    ///     Is tracing enabled
    /// </summary>
    public bool TraceEnabled { get; set; } = false;

    /// <summary>
    ///     Is validation enabled
    /// </summary>
    public bool ValidationEnabled { get; set; } = true;

    /// <summary>
    ///     Optional <see cref="IServiceProvider" /> to use for resolving services
    /// </summary>
    public IServiceProvider? ServiceProvider { get; set; }
}
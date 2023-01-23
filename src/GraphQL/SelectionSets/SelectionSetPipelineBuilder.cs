namespace Tanka.GraphQL.SelectionSets;

public class SelectionSetPipelineBuilder
{
    private readonly List<Func<SelectionSetDelegate, SelectionSetDelegate>> _components = new();

    public SelectionSetPipelineBuilder Use(Func<SelectionSetDelegate, SelectionSetDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public SelectionSetDelegate Build()
    {
        SelectionSetDelegate pipeline = _ => throw new QueryException(
            "Request execution pipeline error. No middleware returned any results.")
        {
            Path = new()
        };

        for (var c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }
}
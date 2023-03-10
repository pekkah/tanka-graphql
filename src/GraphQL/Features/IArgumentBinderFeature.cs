using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Features;

public interface IArgumentBinderFeature
{
    static IArgumentBinderFeature Default = new ArgumentBinderFeature();

    T? BindInputObject<T>(ResolverContextBase context, string name)
        where T : new();

    IEnumerable<T?>? BindInputObjectList<T>(ResolverContextBase context, string name) where T : new();
}
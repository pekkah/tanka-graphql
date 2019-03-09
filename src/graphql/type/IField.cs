using System.Collections.Generic;

namespace tanka.graphql.type
{
    public interface IField : IDirectives, IDeprecable, IDescribable
    {
        IType Type { get; set; }

        IEnumerable<KeyValuePair<string, Argument>> Arguments { get; set; }

        Meta Meta { get; set; }

        Argument GetArgument(string name);

        bool HasArgument(string name);
    }
}
using System.Collections.Generic;

namespace tanka.graphql.type
{
    public interface IField : IDeprecable, IDescribable, IHasDirectives
    {
        IType Type { get; set; }

        IEnumerable<KeyValuePair<string, Argument>> Arguments { get; set; }

        Argument GetArgument(string name);
    }
}
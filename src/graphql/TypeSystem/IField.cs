using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public interface IField : IDeprecable, IDescribable, IHasDirectives
    {
        IType Type { get; set; }

        IEnumerable<KeyValuePair<string, Argument>> Arguments { get; set; }

        Argument GetArgument(string name);
    }
}
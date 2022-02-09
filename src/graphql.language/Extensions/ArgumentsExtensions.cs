using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language;

public static class ArgumentsExtensions
{
    public static bool TryGet(
        this Arguments arguments,
        Name argumentName,
        [NotNullWhen(true)] out Argument? argument)
    {
        argument = arguments.SingleOrDefault(a => a.Name == argumentName);

        return argument is not null;
    }
}
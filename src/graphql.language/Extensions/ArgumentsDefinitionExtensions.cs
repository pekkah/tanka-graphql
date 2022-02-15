using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class ArgumentsDefinitionExtensions
{
    public static bool TryGet(
        this ArgumentsDefinition arguments,
        Name argumentName,
        [NotNullWhen(true)] out InputValueDefinition? argument)
    {
        argument = arguments.SingleOrDefault(a => a.Name == argumentName);

        return argument is not null;
    }
}
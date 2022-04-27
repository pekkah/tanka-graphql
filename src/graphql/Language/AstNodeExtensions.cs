using System;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language;

public static class NodeExtensions
{
    public static string ToGraphQL(this INode node)
    {
        return Printer.Print(node);
    }
}
using System;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public static class NodeExtensions
    {
        [Obsolete("Going to get replaced by the new language module renderer when it's ready")]
        public static string ToGraphQL(this INode node)
        {
            return Printer.Print(node);
        }
    }
}
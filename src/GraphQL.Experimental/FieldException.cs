using System;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public class FieldException : Exception
    {
        public FieldException(
            string message,
            NodePath path,
            Location[] locations,
            Exception? inner = default) : base(message, inner)
        {
            Path = path.Segments.ToArray();
            Locations = locations;
        }

        public Location[] Locations { get; set; }

        public object[] Path { get; set; }
    }
}
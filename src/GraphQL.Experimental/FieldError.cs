using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public record FieldError
    {
        public FieldError(string message, object[] path, Location[] locations)
        {
            Message = message;
            Path = path;
            Locations = locations;
        }

        public Location[] Locations { get; init; }

        public object[] Path { get; init; }

        public string Message { get; init; }
    }
}
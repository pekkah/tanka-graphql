using System;
using System.Collections.Generic;
using System.Linq;

namespace tanka.graphql.type
{
    public class DirectiveType : INamedType
    {
        public static DirectiveType Skip = new DirectiveType(
            "skip",
            new[]
            {
                DirectiveLocation.FIELD,
                DirectiveLocation.FRAGMENT_SPREAD,
                DirectiveLocation.INLINE_FRAGMENT
            },
            new Args
            {
                ["if"] = Argument.Arg(ScalarType.NonNullBoolean)
            });

        public static DirectiveType Include = new DirectiveType(
            "include",
            new[]
            {
                DirectiveLocation.FIELD,
                DirectiveLocation.FRAGMENT_SPREAD,
                DirectiveLocation.INLINE_FRAGMENT
            },
            new Args
            {
                ["if"] = Argument.Arg(ScalarType.NonNullBoolean)
            });

        public static IEnumerable<DirectiveLocation> ExecutableLocations = new[]
        {
            DirectiveLocation.QUERY,
            DirectiveLocation.MUTATION,
            DirectiveLocation.SUBSCRIPTION,
            DirectiveLocation.FIELD,
            DirectiveLocation.FRAGMENT_DEFINITION,
            DirectiveLocation.FRAGMENT_SPREAD,
            DirectiveLocation.INLINE_FRAGMENT
        };

        public static IEnumerable<DirectiveLocation> TypeSystemLocations = new[]
        {
            DirectiveLocation.SCHEMA,
            DirectiveLocation.SCALAR,
            DirectiveLocation.OBJECT,
            DirectiveLocation.FIELD_DEFINITION,
            DirectiveLocation.ARGUMENT_DEFINITION,
            DirectiveLocation.INTERFACE,
            DirectiveLocation.UNION,
            DirectiveLocation.ENUM,
            DirectiveLocation.ENUM_VALUE,
            DirectiveLocation.INPUT_OBJECT,
            DirectiveLocation.INPUT_FIELD_DEFINITION
        };

        private readonly Args _arguments = new Args();
        private readonly List<DirectiveLocation> _locations;

        public DirectiveType(
            string name,
            IEnumerable<DirectiveLocation> locations,
            Args arguments = null,
            Meta meta = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Meta = meta ?? new Meta();
            _locations = locations?.ToList() ?? throw new ArgumentNullException(nameof(locations));

            if (arguments != null)
                foreach (var argument in arguments)
                    _arguments[argument.Key] = argument.Value;
        }

        public Meta Meta { get; }

        public IEnumerable<KeyValuePair<string, Argument>> Arguments => _arguments;

        public IEnumerable<DirectiveLocation> Locations => _locations;

        public bool IsExecutable => _locations.Any(l => ExecutableLocations.Contains(l));

        public bool IsTypeSystem => _locations.Any(l => TypeSystemLocations.Contains(l));

        public string Name { get; }

        public Argument GetArgument(string name)
        {
            if (!_arguments.ContainsKey(name))
                return null;

            return _arguments[name];
        }

        public bool HasArgument(string name)
        {
            return _arguments.ContainsKey(name);
        }
    }

    public enum DirectiveLocation
    {
        QUERY,
        MUTATION,
        SUBSCRIPTION,
        FIELD,
        FRAGMENT_DEFINITION,
        FRAGMENT_SPREAD,
        INLINE_FRAGMENT,

        SCHEMA,
        SCALAR,
        OBJECT,
        FIELD_DEFINITION,
        ARGUMENT_DEFINITION,
        INTERFACE,
        UNION,
        ENUM,
        ENUM_VALUE,
        INPUT_OBJECT,
        INPUT_FIELD_DEFINITION,
    }
}
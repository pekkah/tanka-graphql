using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem
{
    public class ScalarType : INamedType, IEquatable<ScalarType>, IEquatable<INamedType>, IDescribable,
        IHasDirectives
    {
        public static ScalarType Boolean = new(
            "Boolean",
            "The `Boolean` scalar type represents `true` or `false`");

        public static ScalarType Float = new(
            "Float",
            "The `Float` scalar type represents signed double-precision fractional values" +
            " as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)");

        public static ScalarType ID = new(
            "ID",
            "The ID scalar type represents a unique identifier, often used to refetch an object" +
            " or as the key for a cache. The ID type is serialized in the same way as a String; " +
            "however, it is not intended to be human‐readable. While it is often numeric, it " +
            "should always serialize as a String.");

        public static ScalarType Int = new(
            "Int",
            "The `Int` scalar type represents non-fractional signed whole numeric values");

        public static ScalarType String = new(
            "String",
            "The `String` scalar type represents textual data, represented as UTF-8" +
            " character sequences. The String type is most often used by GraphQL to" +
            " represent free-form human-readable text.");

        public static NonNull NonNullBoolean = new(Boolean);
        public static NonNull NonNullFloat = new(Float);
        public static NonNull NonNullID = new(ID);
        public static NonNull NonNullInt = new(Int);
        public static NonNull NonNullString = new(String);

        public static IEnumerable<(ScalarType Type, IValueConverter Converter)> Standard =
            new List<(ScalarType Type, IValueConverter Converter)>
            {
                (String, new StringConverter()),
                (Int, new IntConverter()),
                (Float, new DoubleConverter()),
                (Boolean, new BooleanConverter()),
                (ID, new IdConverter())
            };


        private readonly DirectiveList _directives;


        public ScalarType(
            string name,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            Name = name;
            Description = description ?? string.Empty;
            _directives = new DirectiveList(directives);
        }

        public static IReadOnlyDictionary<string, IValueConverter> StandardConverters => Standard
            .ToDictionary(tc => tc.Type.Name, tc => tc.Converter);

        protected IValueConverter Converter { get; }

        public string Description { get; }

        public bool Equals(INamedType other)
        {
            return Equals((object) other);
        }

        public bool Equals(ScalarType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public IEnumerable<DirectiveInstance> Directives => _directives;

        public DirectiveInstance GetDirective(string name)
        {
            return _directives.GetDirective(name);
        }

        public bool HasDirective(string name)
        {
            return _directives.HasDirective(name);
        }

        public string Name { get; }

        public override string ToString()
        {
            return $"{Name}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((ScalarType) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public static bool operator ==(ScalarType left, ScalarType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScalarType left, ScalarType right)
        {
            return !Equals(left, right);
        }

        internal static IValueConverter GetStandardConverter(string scalarName)
        {
            var scalar = Standard.SingleOrDefault(s => s.Type.Name == scalarName);

            if (scalar.Type == null)
                return null;

            return scalar.Converter;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem
{
    public class ScalarType : INamedType, IEquatable<ScalarType>, IEquatable<INamedType>, IDescribable,
        IHasDirectives
    {
        public static ScalarType Boolean = new ScalarType(
            "Boolean",
            "The `Boolean` scalar type represents `true` or `false`");

        public static ScalarType Float = new ScalarType(
            "Float",
            "The `Float` scalar type represents signed double-precision fractional values" +
            " as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)");

        public static ScalarType ID = new ScalarType(
            "ID",
            "The ID scalar type represents a unique identifier, often used to refetch an object" +
            " or as the key for a cache. The ID type is serialized in the same way as a String; " +
            "however, it is not intended to be human‐readable. While it is often numeric, it " +
            "should always serialize as a String.");

        public static ScalarType Int = new ScalarType(
            "Int",
            "The `Int` scalar type represents non-fractional signed whole numeric values");

        public static ScalarType String = new ScalarType(
            "String",
            "The `String` scalar type represents textual data, represented as UTF-8" +
            " character sequences. The String type is most often used by GraphQL to" +
            " represent free-form human-readable text.");

        public static NonNull NonNullBoolean = new NonNull(Boolean);
        public static NonNull NonNullFloat = new NonNull(Float);
        public static NonNull NonNullID = new NonNull(ID);
        public static NonNull NonNullInt = new NonNull(Int);
        public static NonNull NonNullString = new NonNull(String);

        public static IEnumerable<(ScalarType Type, IValueConverter Converter)> Standard =
            new List<(ScalarType Type, IValueConverter Converter)>()
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

        protected IValueConverter Converter { get; }

        public string Description { get; }

        public IEnumerable<DirectiveInstance> Directives => _directives;

        public DirectiveInstance GetDirective(string name)
        {
            return _directives.GetDirective(name);
        }

        public bool HasDirective(string name)
        {
            return _directives.HasDirective(name);
        }

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
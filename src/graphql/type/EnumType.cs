using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using tanka.graphql.type.converters;
using GraphQLParser.AST;

namespace tanka.graphql.type
{
    public class EnumType : INamedType, IValueConverter
    {
        private readonly EnumValues _values = new EnumValues();

        public EnumType(
            string name, 
            EnumValues values,
            Meta meta = null)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            Meta = meta ?? new Meta();

            foreach (var enumValue in values)
            {
                var value = enumValue.Value ?? new Meta();
                _values[enumValue.Key.ToUpperInvariant()] = value;
            }
        }

        public Meta Meta { get; set; }

        public string Name { get; }

        public IEnumerable<KeyValuePair<string, Meta>> Values => _values;

        public object Serialize(object value)
        {
            var enumValue = _values.SingleOrDefault(v => v.Key == value?
                                                             .ToString()
                                                             .ToUpperInvariant());
            return enumValue.Key;
        }

        public object ParseValue(object input)
        {
            if (input == null)
                return null;

            var stringInput = Convert.ToString(input, CultureInfo.InvariantCulture)
                ?.ToUpperInvariant();

            if (stringInput == null || !_values.ContainsKey(stringInput)) return null;

            var value = _values.SingleOrDefault(v => v.Key == stringInput);
            return value.Key;
        }

        public object ParseLiteral(GraphQLScalarValue input)
        {
            var value = _values.SingleOrDefault(v => v.Key == input.Value?.ToUpperInvariant());
            return value.Key;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class EnumValues : Dictionary<string, Meta>
    {
        public EnumValues()
        {
            
        }

        public EnumValues(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                this[value] = null;
            }
        }
    }
}
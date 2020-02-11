using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class ListFacts
    {
        private IEnumerable<KeyValuePair<string, InputObjectField>> GetInputObjectFields(string type)
        {
            return null;
        }

        private IValueConverter GetValueConverter(string type)
        {
            return ScalarType.Standard.Single(c => c.Type.Name == type).Converter;
        }

        [Fact]
        public void Coerce_ListOfInts()
        {
            /* Given */
            var itemType = ScalarType.Int;
            var listType = new List(itemType);
            var values = new object[] {1, 2, 3};
            
            /* When */
            var coercedRawResult = Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType);


            /* Then */
            var coercedResult = Assert.IsAssignableFrom<IEnumerable<object>>(coercedRawResult);
            Assert.Equal(values, coercedResult);
        }

        [Fact]
        public void Coerce_null_list()
        {
            /* Given */
            var itemType = ScalarType.Int;
            var listType = new List(itemType);
            object values = null;

            /* When */
            var coercedRawResult = Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType);


            /* Then */
            Assert.Null(coercedRawResult);
        }

        [Fact]
        public void Coerce_ListOfInts_with_null_item()
        {
            /* Given */
            var itemType = ScalarType.Int;
            var listType = new List(itemType);
            var values = new object[] {1, 2, null};
            
            /* When */
            var coercedRawResult = Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType);


            /* Then */
            var coercedResult = Assert.IsAssignableFrom<IEnumerable<object>>(coercedRawResult);
            Assert.Equal(values, coercedResult);
        }

        [Fact]
        public void Coerce_NonNull_ListOfInts()
        {
            /* Given */
            var itemType = ScalarType.Int;
            var listType = new NonNull(new List(itemType));
            var values = new object[] {1, 2, 3};
            
            /* When */
            var coercedRawResult = Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType);


            /* Then */
            var coercedResult = Assert.IsAssignableFrom<IEnumerable<object>>(coercedRawResult);
            Assert.Equal(values, coercedResult);
        }

        [Fact]
        public void Coerce_NonNull_ListOfInts_as_null()
        {
            /* Given */
            var itemType = ScalarType.Int;
            var listType = new NonNull(new List(itemType));
            object values = null;
            
            /* When */
            /* Then */
            Assert.Throws<ValueCoercionException>(()=>Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType));
        }

        [Fact]
        public void Coerce_NonNull_ListOfInts_with_null_item()
        {
            /* Given */
            var itemType = ScalarType.Int;
            var listType = new NonNull(new List(itemType));
            var values = new object[] {1, 2, null};
            
            /* When */
            var coercedRawResult = Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType);


            /* Then */
            var coercedResult = Assert.IsAssignableFrom<IEnumerable<object>>(coercedRawResult);
            Assert.Equal(values, coercedResult);
        }

        [Fact]
        public void Coerce_ListOf_NonNullInts()
        {
            /* Given */
            var itemType = ScalarType.NonNullInt;
            var listType = new List(itemType);
            var values = new object[] {1, 2, 3};
            
            /* When */
            var coercedRawResult = Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType);


            /* Then */
            var coercedResult = Assert.IsAssignableFrom<IEnumerable<object>>(coercedRawResult);
            Assert.Equal(values, coercedResult);
        }

        [Fact]
        public void Coerce_ListOf_NonNullInts_with_null_list()
        {
            /* Given */
            var itemType = ScalarType.NonNullInt;
            var listType = new List(itemType);
            object values = null;
            
            /* When */
            var coercedRawResult = Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType);


            /* Then */
            Assert.Null(coercedRawResult);
        }

        [Fact]
        public void Coerce_ListOf_NonNullInts_with_null_item()
        {
            /* Given */
            var itemType = ScalarType.NonNullInt;
            var listType = new List(itemType);
            var values = new object[] {1, 2, null};
            
            /* When */
            /* Then */
            Assert.Throws<ValueCoercionException>(()=>Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType));
        }

        [Fact]
        public void Coerce_NonNull_ListOf_NonNullInts()
        {
            /* Given */
            var itemType = ScalarType.NonNullInt;
            var listType = new NonNull(new List(itemType));
            var values = new object[] {1, 2, 3};
            
            /* When */
            var coercedRawResult = Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType);


            /* Then */
            var coercedResult = Assert.IsAssignableFrom<IEnumerable<object>>(coercedRawResult);
            Assert.Equal(values, coercedResult);
        }

        [Fact]
        public void Coerce_NonNull_ListOf_NonNullInts_with_null()
        {
            /* Given */
            var itemType = ScalarType.NonNullInt;
            var listType = new NonNull(new List(itemType));
            object values = null;
            
            /* When */
            /* Then */
            Assert.Throws<ValueCoercionException>(()=>Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType));
        }

        [Fact]
        public void Coerce_NonNull_ListOf_NonNullInts_with_null_item()
        {
            /* Given */
            var itemType = ScalarType.NonNullInt;
            var listType = new NonNull(new List(itemType));
            object values = new object[] {1, 2, null};;
            
            /* When */
            /* Then */
            Assert.Throws<ValueCoercionException>(()=>Values.CoerceValue(
                GetInputObjectFields,
                GetValueConverter,
                values,
                listType));
        }

        [Fact]
        public void Define_list()
        {
            /* Given */
            var itemType = ScalarType.Int;

            /* When */
            var list = new List(itemType);

            /* Then */
            Assert.Equal(itemType, list.OfType);
        }
    }
}
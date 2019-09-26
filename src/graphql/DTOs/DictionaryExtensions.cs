using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Tanka.GraphQL.DTOs
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToNestedDictionary(this JObject json)
        {
            var propertyValuePairs = json.ToObject<Dictionary<string, object>>();
            ProcessJObjectProperties(propertyValuePairs);
            ProcessJArrayProperties(propertyValuePairs);
            return propertyValuePairs;
        }

        private static object[] ToArray(this JArray array)
        {
            return array.ToObject<object[]>().Select(ProcessArrayEntry).ToArray();
        }

        private static void ProcessJObjectProperties(IDictionary<string, object> propertyValuePairs)
        {
            var objectPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value is JObject
                select propertyName).ToList();

            objectPropertyNames.ForEach(propertyName =>
                propertyValuePairs[propertyName] = ToNestedDictionary((JObject) propertyValuePairs[propertyName]));
        }

        private static void ProcessJArrayProperties(IDictionary<string, object> propertyValuePairs)
        {
            var arrayPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value is JArray
                select propertyName).ToList();

            arrayPropertyNames.ForEach(propertyName =>
                propertyValuePairs[propertyName] = ToArray((JArray) propertyValuePairs[propertyName]));
        }

        private static object ProcessArrayEntry(object value)
        {
            switch (value)
            {
                case JObject o:
                    return ToNestedDictionary(o);
                case JArray array:
                    return ToArray(array);
            }

            return value;
        }
    }
}
using System;
using System.Collections.Generic;
using fugu.graphql.server.utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace fugu.graphql.server.subscriptions
{
    /// <summary>
    ///     Payload of start message
    /// </summary>
    public class OperationMessagePayload
    {
        /// <summary>
        ///     Query, mutation or subsciption query
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        ///     Variables
        /// </summary>
        [JsonConverter(typeof(VariableConverter))]
        public Dictionary<string, object> Variables { get; set; }

        /// <summary>
        ///     Operation name
        /// </summary>
        public string OperationName { get; set; }
    }

    internal class VariableConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new Dictionary<string, object>();

            var obj = JObject.Load(reader);
            return obj.ToVariableDictionary();
        }
    }
}
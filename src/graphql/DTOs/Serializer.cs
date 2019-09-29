using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL.DTOs
{
    public class Serializer
    {
        private readonly JsonSerializerOptions _options;

        public Serializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new ObjectDictionaryConverter()
                }
            };
        }

        public byte[] Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes<T>(obj, _options);
        }

        public T Deserialize<T>(byte[] json)
        {
            return (T) JsonSerializer.Deserialize(json, typeof(T), _options);
        }
    }
}
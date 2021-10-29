using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CacheService.Tests
{
    public class ErrorInSerializeConverter : JsonConverter<ErrorInSerialize>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            throw new NotImplementedException();
        }

        public override ErrorInSerialize? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ErrorInSerialize value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
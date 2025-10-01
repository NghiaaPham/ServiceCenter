using System.Text.Json;
using System.Text.Json.Serialization;

namespace EVServiceCenter.Infrastructure.JsonConverters
{
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private readonly string _format;

        public TimeOnlyJsonConverter(string format = "HH:mm")
        {
            _format = format;
        }

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (TimeOnly.TryParse(value, out var time))
            {
                return time;
            }

            throw new JsonException($"Invalid time format. Expected format: {_format}");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }
}

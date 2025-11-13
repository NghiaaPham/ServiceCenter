using System.Globalization;
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
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new JsonException($"Invalid time format. Expected format: {_format}");
            }

            // Loại bỏ timezone indicator (Z) nếu có vì TimeOnly không hỗ trợ timezone
            var timeString = value.TrimEnd('Z', 'z');

            // Thử parse với nhiều format phổ biến
            if (TimeOnly.TryParseExact(timeString, 
                new[] { "HH:mm:ss.fff", "HH:mm:ss.ff", "HH:mm:ss.f", "HH:mm:ss", "HH:mm", _format }, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var time))
            {
                return time;
            }

            // Fallback: thử parse với culture mặc định
            if (TimeOnly.TryParse(timeString, CultureInfo.InvariantCulture, out time))
            {
                return time;
            }

            throw new JsonException($"Invalid time format. Expected format: {_format}. Received: {value}");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }
}

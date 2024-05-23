using System.Globalization;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest.ApiMessages;

public class ApiDateTimeConverter : JsonConverter<DateTime>
{
    const string DateFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.TryParse(reader.GetString(), out DateTime date)
            ? date : throw new JsonException($"Unable to convert data to DateTime.");

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
}
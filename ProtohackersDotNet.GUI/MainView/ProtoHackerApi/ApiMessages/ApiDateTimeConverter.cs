using System.Globalization;

namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;

public class ApiDateTimeConverter : JsonConverter<DateTimeOffset>
{
    const string DateFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTimeOffset.TryParse(reader.GetString(), out DateTimeOffset date)
            ? date : throw new JsonException($"Unable to convert data to DateTime");

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
}
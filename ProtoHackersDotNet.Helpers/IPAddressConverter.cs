using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProtoHackersDotNet.Helpers;

public class IPAddressConverter : JsonConverter<IPAddress>
{
    public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => IPAddress.TryParse(reader.GetString(), out IPAddress? ip)
            ? ip : throw new JsonException($"Unable to convert \"{reader.GetString()}\" to IPAddress");

    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
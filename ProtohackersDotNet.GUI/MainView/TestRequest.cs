using System.Text.Json.Serialization;

namespace ProtoHackersDotNet.GUI.MainView;

class TestRequest
{
    public required int Problem { get; init; }

    [property: JsonPropertyName("ipaddr")]
    public required string IpAddress { get; init; }

    public required uint Port { get; init; }
}
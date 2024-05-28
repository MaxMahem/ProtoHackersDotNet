namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;

public class TestApiRequest
{
    public required int Problem { get; init; }

    [JsonPropertyName("ipaddr"), JsonConverter(typeof(IPAddressConverter))]
    public required IPAddress Ip { get; init; }

    public required uint Port { get; init; }
}
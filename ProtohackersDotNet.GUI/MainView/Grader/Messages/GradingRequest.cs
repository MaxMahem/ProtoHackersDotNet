namespace ProtoHackersDotNet.GUI.MainView.Grader.Messages;

public class GradingRequest
{
    public required int Problem { get; init; }

    [JsonPropertyName("ipaddr"), JsonConverter(typeof(IPAddressConverter))]
    public required IPAddress Ip { get; init; }

    public required uint Port { get; init; }
}
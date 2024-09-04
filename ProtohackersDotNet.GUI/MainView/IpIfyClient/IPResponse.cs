namespace ProtoHackersDotNet.GUI.MainView.IpIfyClient;

/// <summary>Represents a response from the IpIfy IP address API.</summary>
public class IPResponse
{
    /// <summary>IP address returned from the response.</summary>
    [JsonConverter(typeof(IPAddressConverter)), JsonPropertyName("ip")] public required IPAddress Ip { get; init; }
}

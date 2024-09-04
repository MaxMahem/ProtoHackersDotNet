using ProtoHackersDotNet.GUI.MainView.Grader;

namespace ProtoHackersDotNet.GUI.MainView.IpIfyClient;

public class IpIfyClientOptions
{
    /// <summary>IpIfy Url to get an IPv4 or IPv6 address.</summary>
    [AbsoluteUri] public required Uri IPv46Url { get; init; }

    /// <summary>IpIfy Url to get an IPv4 address.</summary>
    [AbsoluteUri] public required Uri IPv4Url { get; init; }

    /// <summary>IpIfy Url to get an IPv6 address.</summary>
    [AbsoluteUri] public required Uri IPv6Url { get; init; }
}
namespace ProtoHackersDotNet.Servers.Interfaces.Client;

/// <summary>Possible status of a client connection.</summary>
public enum ConnectionStatus
{
    /// <summary>The client is conected normally.</summary>
    Connected,
    /// <summary>The client was disconnected gracefully.</summary>
    Disconnected,
    /// <summary>The client was disconnected forcefully.</summary>
    Terminated,
}